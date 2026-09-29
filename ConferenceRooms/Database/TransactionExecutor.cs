using Microsoft.AspNetCore.Connections;
using Npgsql;

namespace ConferenceRooms.Database
{
    // Клас для створення та виконання транзакцій у базі даних PostgreSQL. Використовує фабрику підключень для створення з'єднання та керування транзакціями.
    public class TransactionExecutor
    {
        private readonly DbConnectionFactory _factory;

        public TransactionExecutor(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task ExecuteAsync(Func<NpgsqlConnection, NpgsqlTransaction, Task> action)
        {
            await using var conn = _factory.Create();
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await action(conn, tx);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task ExecuteAsync(Func<NpgsqlConnection, Task> action)
        {
            await using var conn = _factory.Create();
            await conn.OpenAsync();
            await action(conn);
        }

        public async Task<T> ExecuteAsync<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> action)
        {
            await using var conn = _factory.Create();
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                var result = await action(conn, tx);

                await tx.CommitAsync();

                return result;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<NpgsqlConnection, Task<T>> action)
        {
            await using var conn = _factory.Create();
            await conn.OpenAsync();
            return await action(conn);
        }
    }
}
