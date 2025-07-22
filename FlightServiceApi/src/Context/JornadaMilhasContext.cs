using FlightServiceApi.src.Flights;
using FlightServiceApi.src.Tickets;
using Microsoft.EntityFrameworkCore;

namespace FlightServiceApi.src.Context;

public class JornadaMilhasContext:DbContext
{

    public JornadaMilhasContext(DbContextOptions<JornadaMilhasContext> options):base(options)
    {
        
    }

    public DbSet<Voo> Voos { get; set; }
    public DbSet<Passagem> Passagens { get; set; }

}
