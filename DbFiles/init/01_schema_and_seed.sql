-- 01_schema_and_seed.sql: runs after 00_create_app_role.sh, as accounts_admin
SET TIME ZONE 'UTC';

-- ─── Lookup tables (ids assigned explicitly) ─────────────────────────────

CREATE TABLE account_statuses (
                                  account_status_id  integer     NOT NULL,
                                  title              varchar(50) NOT NULL,

                                  CONSTRAINT pk_account_statuses PRIMARY KEY (account_status_id),
                                  CONSTRAINT uq_account_statuses_title UNIQUE (title)
);

CREATE TABLE transaction_types (
                                   transaction_type_id  integer     NOT NULL,
                                   title                varchar(50) NOT NULL,

                                   CONSTRAINT pk_transaction_types PRIMARY KEY (transaction_type_id),
                                   CONSTRAINT uq_transaction_types_title UNIQUE (title)
);

-- ─── Core tables ─────────────────────────────────────────────────────────

CREATE TABLE customers (
                           customer_id       integer      GENERATED ALWAYS AS IDENTITY,
                           first_name        varchar(100) NOT NULL,
                           last_name         varchar(100) NOT NULL,
                           identity_subject  varchar(255) NULL,          -- Auth0 "sub" claim

                           CONSTRAINT pk_customers PRIMARY KEY (customer_id),
                           CONSTRAINT uq_customers_identity_subject UNIQUE (identity_subject)
);

CREATE TABLE accounts (
                          account_id         integer       GENERATED ALWAYS AS IDENTITY,
                          customer_id        integer       NOT NULL,
                          balance            numeric(19,4) NOT NULL DEFAULT 0,  -- only changed together with a transaction insert
                          overdraft_limit    numeric(19,4) NOT NULL DEFAULT 0,
                          date_opened        timestamptz   NOT NULL,
                          currency_code      char(3)       NOT NULL,             -- ISO 4217
                          account_status_id  integer       NOT NULL,

                          CONSTRAINT pk_accounts PRIMARY KEY (account_id),
                          CONSTRAINT fk_accounts_customer FOREIGN KEY (customer_id) REFERENCES customers (customer_id),
                          CONSTRAINT fk_accounts_account_status FOREIGN KEY (account_status_id) REFERENCES account_statuses (account_status_id),
                          CONSTRAINT ck_accounts_overdraft_limit CHECK (overdraft_limit >= 0),
                          CONSTRAINT ck_accounts_currency_code CHECK (currency_code ~ '^[A-Z]{3}$')
    );

CREATE TABLE transactions (
                              transaction_id       bigint        GENERATED ALWAYS AS IDENTITY,
                              idempotency_key      uuid          NOT NULL,
                              account_id           integer       NOT NULL,
                              occurred_at          timestamptz   NOT NULL,
                              amount               numeric(19,4) NOT NULL,          -- signed: + in, − out
                              counterparty_name    varchar(140)  NULL,
                              transaction_type_id  integer       NOT NULL,
                              description          varchar(140)  NOT NULL,

                              CONSTRAINT pk_transactions PRIMARY KEY (transaction_id),
                              CONSTRAINT uq_transactions_idempotency_key UNIQUE (idempotency_key),
                              CONSTRAINT fk_transactions_account FOREIGN KEY (account_id) REFERENCES accounts (account_id),
                              CONSTRAINT fk_transactions_transaction_type FOREIGN KEY (transaction_type_id) REFERENCES transaction_types (transaction_type_id),
                              CONSTRAINT ck_transactions_amount_not_zero CHECK (amount <> 0)
);

-- ─── Indexes ─────────────────────────────────────────────────────────────

-- Statement query: one account's transactions in date order (also covers the account_id FK)
CREATE INDEX ix_transactions_account_occurred ON transactions (account_id, occurred_at, transaction_id);

-- Remaining FK columns (Postgres does not index foreign keys automatically)
CREATE INDEX ix_accounts_customer_id         ON accounts (customer_id);
CREATE INDEX ix_accounts_account_status_id   ON accounts (account_status_id);
CREATE INDEX ix_transactions_transaction_type_id ON transactions (transaction_type_id);

-- ─── Functions (replace the GetAccount / GetAccounts procedures) ─────────

CREATE FUNCTION get_account(p_account_id integer)
    RETURNS TABLE (
                      account_id       integer,
                      first_name       varchar,
                      last_name        varchar,
                      balance          numeric,
                      overdraft_limit  numeric,
                      currency_code    char(3)
                  )
    LANGUAGE sql STABLE
AS $$
SELECT a.account_id, c.first_name, c.last_name,
       a.balance, a.overdraft_limit, a.currency_code
FROM accounts a
         JOIN customers c ON c.customer_id = a.customer_id
WHERE a.account_id = p_account_id;
$$;

CREATE FUNCTION get_accounts()
    RETURNS TABLE (
                      account_id       integer,
                      first_name       varchar,
                      last_name        varchar,
                      balance          numeric,
                      overdraft_limit  numeric,
                      currency_code    char(3)
                  )
    LANGUAGE sql STABLE
AS $$
SELECT a.account_id, c.first_name, c.last_name,
       a.balance, a.overdraft_limit, a.currency_code
FROM accounts a
         JOIN customers c ON c.customer_id = a.customer_id
ORDER BY a.account_id;
$$;

-- ─── Seed: lookups ───────────────────────────────────────────────────────

INSERT INTO account_statuses (account_status_id, title) VALUES
                                                            (1, 'Open'),
                                                            (2, 'Frozen'),
                                                            (3, 'Closed');

INSERT INTO transaction_types (transaction_type_id, title) VALUES
                                                               (1, 'Card payment'),
                                                               (2, 'Deposit'),
                                                               (3, 'Transfer'),
                                                               (4, 'Fee'),
                                                               (5, 'Opening balance'),
                                                               (6, 'Cash withdrawal'),
                                                               (7, 'Cheque payment'),
                                                               (8, 'Direct Debit');

-- ─── Seed: customers and accounts ────────────────────────────────────────

INSERT INTO customers (first_name, last_name) VALUES
                                                  ('John', 'Smith'), ('Emma', 'Johnson'), ('John', 'Brown'), ('Olivia', 'Davis'),
                                                  ('William', 'Miller'), ('Sophia', 'Wilson'), ('James', 'Moore'), ('Isabella', 'Taylor'),
                                                  ('Benjamin', 'Anderson'), ('Mia', 'Thomas');

-- balance starts at 0 and is set from the transactions at the end
INSERT INTO accounts (customer_id, overdraft_limit, date_opened, currency_code, account_status_id) VALUES
                                                                                                       (1, 500.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (2, 1000.00, '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (3, 200.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (4, 1500.00, '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (5, 100.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (6, 300.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (7, 1200.00, '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (8, 250.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (9, 800.00,  '2025-12-01 09:00:00', 'EUR', 1),
                                                                                                       (10, 150.00, '2025-12-01 09:00:00', 'EUR', 1);

-- ─── Seed: transactions ──────────────────────────────────────────────────
-- Inserted in date order so transaction_id follows occurred_at.

INSERT INTO transactions (idempotency_key, account_id, occurred_at, amount,
                          counterparty_name, transaction_type_id, description)
SELECT uuidv7(), t.account_id, t.occurred_at::timestamptz, t.amount,
       t.counterparty_name, t.transaction_type_id, t.description
FROM (VALUES
          -- Opening balances
          (1,  '2025-12-01 09:00:00', 1500.50,  NULL, 5, 'Opening balance'),
          (2,  '2025-12-01 09:00:00', 3200.00,  NULL, 5, 'Opening balance'),
          (3,  '2025-12-01 09:00:00', 250.75,   NULL, 5, 'Opening balance'),
          (4,  '2025-12-01 09:00:00', 7800.20,  NULL, 5, 'Opening balance'),
          (5,  '2025-12-01 09:00:00', 300.00,   NULL, 5, 'Opening balance'),
          (6,  '2025-12-01 09:00:00', 920.10,   NULL, 5, 'Opening balance'),
          (7,  '2025-12-01 09:00:00', 4300.00,  NULL, 5, 'Opening balance'),
          (8,  '2025-12-01 09:00:00', 610.45,   NULL, 5, 'Opening balance'),
          (9,  '2025-12-01 09:00:00', 2750.00,  NULL, 5, 'Opening balance'),
          (10, '2025-12-01 09:00:00', 400.00,   NULL, 5, 'Opening balance'),
          -- January activity (old debit → card payment, old credit → deposit)
          (1,  '2026-01-01 10:15:00', -1200.50, 'Pies R Us',        1, 'Card payment'),
          (1,  '2026-01-02 12:30:00', 75.00,    NULL,                  2, 'Deposit'),
          (2,  '2026-01-03 09:00:00', -200.00,  'Supermarché du Port', 1, 'Card payment'),
          (2,  '2026-01-04 14:45:00', 5000.25,  'Cake R Us',            2, 'Salary'),
          (3,  '2026-01-05 16:20:00', -310.10,  'Donuts Inc',            1, 'Card payment'),
          (3,  '2026-01-06 11:10:00', 90.00,    NULL,                  2, 'Deposit'),
          (4,  '2026-01-07 08:05:00', -450.75,  'Librairie Centrale',  1, 'Card payment'),
          (4,  '2026-01-08 19:25:00', 120.00,   NULL,                  2, 'Deposit'),
          (5,  '2026-01-09 13:15:00', -60.00,   'Marché Bio',          1, 'Card payment'),
          (5,  '2026-01-10 17:40:00', 30.50,    NULL,                  2, 'Deposit'),
          (6,  '2026-01-11 10:10:00', -500.00,  'Rail Express',        1, 'Card payment'),
          (6,  '2026-01-12 15:55:00', 200.00,   NULL,                  2, 'Deposit'),
          (7,  '2026-01-13 09:35:00', -80.80,   'Supermarché du Port', 1, 'Card payment'),
          (7,  '2026-01-14 18:20:00', 45.00,    NULL,                  2, 'Deposit'),
          (8,  '2026-01-15 11:00:00', -220.00,  'Sport Atlantique',    1, 'Card payment'),
          (8,  '2026-01-16 16:45:00', 100.00,   NULL,                  2, 'Deposit'),
          (9,  '2026-01-17 14:10:00', -330.30,  'Maison & Meuble',     1, 'Card payment'),
          (9,  '2026-01-18 12:25:00', 1500.00,  'Cake R US',            2, 'Salary'),
          (10, '2026-01-19 08:50:00', -410.00,  'Hi-Fi Océan',         1, 'Card payment'),
          (10, '2026-01-20 20:15:00', 210.75,   NULL,                  2, 'Deposit'),
          (1,  '2026-01-21 07:30:00', -95.00,   'Station Service Ouest', 1, 'Card payment'),
          (2,  '2026-01-22 13:05:00', 60.00,    NULL,                  2, 'Deposit'),
          (3,  '2026-01-23 15:45:00', -180.00,  'Marché Bio',          1, 'Card payment'),
          (4,  '2026-01-24 10:20:00', 70.00,    NULL,                  2, 'Deposit'),
          (5,  '2026-01-25 11:35:00', -250.00,  'Boutique en Ligne',   1, 'Card payment'),
          (6,  '2026-01-26 17:50:00', 90.00,    NULL,                  2, 'Deposit'),
          (7,  '2026-01-27 09:15:00', -300.00,  'Donuts Inc',      1, 'Card payment'),
          (8,  '2026-01-28 14:40:00', 110.00,   NULL,                  2, 'Deposit'),
          (9,  '2026-01-29 16:55:00', -275.00,  'Pies R Us',            1, 'Card payment'),
          (10, '2026-01-30 18:10:00', 130.00,   NULL,                  2, 'Deposit')
     ) AS t(account_id, occurred_at, amount, counterparty_name, transaction_type_id, description)
ORDER BY t.occurred_at, t.account_id;

-- ─── Seed: balances ──────────────────────────────────────────────────────
-- The seed bypasses the application, so set each balance from its transactions once.

UPDATE accounts a
SET balance = s.total
    FROM (
    SELECT account_id, SUM(amount) AS total
    FROM transactions
    GROUP BY account_id
) s
WHERE s.account_id = a.account_id;