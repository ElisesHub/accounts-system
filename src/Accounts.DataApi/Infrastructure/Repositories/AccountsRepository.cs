using System.Data;
using AccountsAPI.Application.Interfaces;
using AccountsAPI.Domain.Entities;
using MySqlConnector;

namespace AccountsAPI.Infrastructure.Repositories;

public class AccountsRepository(IConfiguration configuration) : IAccountsRepository
{
    public async Task<Account?> GetAccountAsync(string accountId)
    {
        Account account = new Account();
        string sqlconnectionstring =
            configuration.GetConnectionString("AccountsDb");

        using (MySqlConnection con = new MySqlConnection(sqlconnectionstring))
        {

            try
            {
                await con.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("GetAccount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_id", accountId);
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader["id"] != DBNull.Value)
                            {
                                account.Id = Convert.ToInt32(reader["id"]);
                            }

                            if (reader["firstname"] != DBNull.Value)
                            {
                                account.FirstName = reader["firstname"].ToString();
                            }

                            if (reader["lastname"] != DBNull.Value)
                            {
                                account.LastName = reader["lastname"].ToString();
                            }

                            if (reader["currentBalance"] != DBNull.Value)
                            {
                                account.Balance = Convert.ToDecimal(reader["currentBalance"]);
                            }

                            if (reader["overdraftLimit"] != DBNull.Value)
                            {
                                account.OverdraftLimit = Convert.ToDecimal(reader["overdraftLimit"]);
                            }


                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Log this
                throw;
            }
        }

        return account;
    }

    public async Task<List<Account>?> GetAccountsAsync()
    {
        List<Account> list = new List<Account>();

        string sqlconnectionstring =
            configuration.GetConnectionString("AccountsDb");

        using (MySqlConnection con = new MySqlConnection(sqlconnectionstring))
        {
            try
            {
                await con.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("GetAccounts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Account account = new Account();

                            if (reader["id"] != DBNull.Value)
                            {
                                account.Id = Convert.ToInt32(reader["id"]);
                            }

                            if (reader["firstname"] != DBNull.Value)
                            {
                                account.FirstName = reader["firstname"].ToString();
                            }

                            if (reader["lastname"] != DBNull.Value)
                            {
                                account.LastName = reader["lastname"].ToString();
                            }

                            if (reader["currentBalance"] != DBNull.Value)
                            {
                                account.Balance = Convert.ToDecimal(reader["currentBalance"]);
                            }

                            if (reader["overdraftLimit"] != DBNull.Value)
                            {
                                account.OverdraftLimit = Convert.ToDecimal(reader["overdraftLimit"]);
                            }

                            list.Add(account);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: add logging
                throw;
            }
        }

        return list;
    }
}