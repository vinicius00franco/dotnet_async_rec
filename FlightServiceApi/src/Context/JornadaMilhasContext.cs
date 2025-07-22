using dotnet_async_api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace dotnet_async_api.Context;

public class JornadaMilhasContext:DbContext
{

    public JornadaMilhasContext(DbContextOptions<JornadaMilhasContext> options):base(options)
    {
        
    }

    public DbSet<Voo> Voos { get; set; }

}
