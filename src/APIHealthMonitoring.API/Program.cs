using APIHealthMonitoring.Persistence;

namespace APIHealthMonitoring
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------------------------------------------------
            // Service Registration
            // -------------------------------------------------------------------------

            // Registers AppDbContext (SQL Server) and IUnitOfWork.
            // All Persistence layer DI configuration lives in PersistenceServiceRegistration.
            builder.Services.AddPersistenceServices(builder.Configuration);

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // -------------------------------------------------------------------------
            // HTTP Pipeline Configuration
            // -------------------------------------------------------------------------

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}