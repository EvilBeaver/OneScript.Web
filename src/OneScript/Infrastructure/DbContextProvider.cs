/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Microsoft.EntityFrameworkCore;
using OneScript.WebHost.Database;

namespace OneScript.WebHost.Infrastructure
{
    public class DbContextProvider
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DbContextProvider(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }

        public ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }
    }
}