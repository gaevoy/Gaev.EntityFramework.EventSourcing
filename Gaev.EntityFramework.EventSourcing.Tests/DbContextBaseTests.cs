using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Gaev.EntityFramework.EventSourcing.Tests
{
    [TestFixture]
    public class DbContextBaseTests
    {
        private readonly TestConfig _config = new TestConfig();

        [Test]
        public void It_should_create_events()
        {
            // Given
            var db = NewTestDbContext();
            // EnsureCreated(db);
            var company = new Company
            {
                Name = RandomString(),
                Staff = new List<Employee>
                {
                    new Employee {Name = RandomString()},
                    new Employee {Name = RandomString()},
                    new Employee {Name = RandomString()}
                }
            };

            // When
            db.Companies.Add(company);
            db.SaveChanges();

            // Then
            var events = db.Events.OrderByDescending(e => e.Id).Take(4).Select(Mappings.Deserialize).ToList();
            var companyCreated = events.OfType<CompanyCreated>().Single();
            var employeesCreated = events.OfType<EmployeeCreated>().ToList();
            Assert.AreEqual(company.Id, companyCreated.Id);
            Assert.AreEqual(company.Name, companyCreated.Name);
            CollectionAssert.AreEquivalent(company.Staff.Select(e => e.Id), employeesCreated.Select(e => e.Id));
            CollectionAssert.AreEquivalent(company.Staff.Select(e => e.Name), employeesCreated.Select(e => e.Name));
            Assert.IsTrue(employeesCreated.All(e => e.CompanyId == company.Id));
        }

        [Test]
        public void It_should_rollback_atomically()
        {
            // Given
            var db = NewTestDbContext();
            var companies = new[]
            {
                new Company {Name = RandomString()},
                new Company {Name = RandomString()},
                new Company {Name = null}
            };

            // When
            db.Companies.AddRange(companies);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
            }

            // Then
            var events = db.Events
                .OrderByDescending(e => e.Id)
                .Take(companies.Length)
                .Select(Mappings.Deserialize)
                .OfType<CompanyCreated>()
                .ToList();
            CollectionAssert.IsEmpty(events.Where(e => e.Name == companies[0].Name));
            CollectionAssert.IsEmpty(events.Where(e => e.Name == companies[1].Name));
        }

        [Test]
        public void It_should_rollback_atomically_whole_transaction()
        {
            // Given
            var db = NewTestDbContext();
            var companies = new[]
            {
                new Company {Name = RandomString()},
                new Company {Name = RandomString()}
            };

            // When
            try
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    db.Companies.AddRange(companies);
                    db.SaveChanges();

                    db.Companies.Add(new Company {Name = null});
                    db.SaveChanges();

                    tran.Commit();
                }
            }
            catch (DbUpdateException)
            {
            }

            // Then
            var events = db.Events
                .OrderByDescending(e => e.Id)
                .Take(companies.Length)
                .Select(Mappings.Deserialize)
                .OfType<CompanyCreated>()
                .ToList();
            CollectionAssert.IsEmpty(events.Where(e => e.Name == companies[0].Name));
            CollectionAssert.IsEmpty(events.Where(e => e.Name == companies[1].Name));
        }

        [Test, Parallelizable(ParallelScope.None)]
        public async Task It_should_catch_up()
        {
            // Given
            var db = NewTestDbContext();
            db.Events.RemoveRange(db.Events);
            await db.SaveChangesAsync();
            var handled = new List<CompanyCreated>();
            var cancellation = new CancellationTokenSource();
            var running = Subscriptions.CatchUp(evt =>
            {
                lock (handled) handled.Add((CompanyCreated) evt);
                return Task.CompletedTask;
            }, NewTestDbContext(), cancellation.Token);
            var companies = Enumerable.Range(0, 5).Select(_ => new Company {Name = RandomString()}).ToList();
            var companies2 = Enumerable.Range(0, 5).Select(_ => new Company {Name = RandomString()}).ToList();

            // When
            db.Companies.AddRange(companies);
            await db.SaveChangesAsync();
            await Task.Delay(200);
            db.Companies.AddRange(companies2);
            await db.SaveChangesAsync();
            while (true)
            {
                lock (handled)
                    if (handled.Count >= 10)
                        break;
                await Task.Delay(50);
            }

            cancellation.Cancel();
            await running;

            // Then
            var expected = string.Join("|", companies.Union(companies2).Select(e => e.Name));
            Assert.AreEqual(expected, string.Join("|", handled.Select(e => e.Name)));
        }

        private TestDbContext NewTestDbContext()
        {
            var eventMapper = new EntityMapper().Map<Company>(Mappings.Map).Map<Employee>(Mappings.Map)
                .Ignore<Checkpoint>();
            var db = new TestDbContext(_config.ConnectionString, new ChangesBroadcaster(eventMapper));
            return db;
        }

        private string RandomString() => Guid.NewGuid().ToString();

        private void EnsureCreated(DbContextBase context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}