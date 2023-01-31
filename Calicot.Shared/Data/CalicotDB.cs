using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos;
using Calicot.Shared.Models;

namespace Calicot.Shared.Data {
    public class CalicotDB : DbContext
    {
        public CalicotDB (DbContextOptions<CalicotDB> options)
            : base(options)
        {
        }

        public DbSet<Calicot.Shared.Models.Produit> Produits { get; set; } = default!;
    }
}
