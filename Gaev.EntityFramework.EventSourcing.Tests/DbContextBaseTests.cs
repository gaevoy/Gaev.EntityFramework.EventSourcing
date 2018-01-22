using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Gaev.EntityFramework.EventSourcing.Tests
{
    [TestFixture]
    public class DbContextBaseTests
    {
        private readonly TestConfig _config = new TestConfig();

        [Test]
        public void It_should_just_work()
        {
            // Given
            var eventMapper = new EntityMapper().Map<Company>(Mappings.Map).Map<Employee>(Mappings.Map);
            var db = new TestDbContext(_config.ConnectionString, new ChangesBroadcaster(eventMapper));
            // db.EnsureCreated();
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

        private string RandomString() => Guid.NewGuid().ToString();
    }
}