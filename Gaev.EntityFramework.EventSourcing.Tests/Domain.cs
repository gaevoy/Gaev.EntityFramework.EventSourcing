using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Gaev.EntityFramework.EventSourcing.Tests
{
    public class TestDbContext : DbContextBase
    {
        public TestDbContext(string connectionString, IChangesBroadcaster broadcaster) : base(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options, broadcaster)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Checkpoint> Checkpoints { get; set; }
    }

    #region Entities

    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Employee> Staff { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
    
    public class Checkpoint
    {
        public string Id { get; set; }
        public long EventId { get; set; }
    }

    #endregion

    #region Events

    public abstract class DomainEvent
    {
        public DateTimeOffset At { get; set; } = DateTimeOffset.UtcNow;
        public string By { get; set; } = "John Doe";
    }

    public class CompanyCreated: DomainEvent
    {
        public CompanyCreated(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CompanyUpdated: DomainEvent
    {
        public CompanyUpdated(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CompanyDeleted: DomainEvent
    {
        public CompanyDeleted(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    public class EmployeeCreated: DomainEvent
    {
        public EmployeeCreated(int id, string name, int companyId)
        {
            Id = id;
            Name = name;
            CompanyId = companyId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
    }

    public class EmployeeUpdated: DomainEvent
    {
        public EmployeeUpdated(int id, string name, int companyId)
        {
            Id = id;
            Name = name;
            CompanyId = companyId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
    }

    public class EmployeeDeleted: DomainEvent
    {
        public EmployeeDeleted(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    #endregion
    
    public static class Mappings
    {
        public static object Map(Company e, EntityState state)
        {
            switch (state)
            {
                case EntityState.Deleted:
                    return new CompanyDeleted(e.Id);
                case EntityState.Modified:
                    return new CompanyUpdated(e.Id, e.Name);
                case EntityState.Added:
                    return new CompanyCreated(e.Id, e.Name);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public static object Map(Employee e, EntityState state)
        {
            switch (state)
            {
                case EntityState.Deleted:
                    return new EmployeeDeleted(e.Id);
                case EntityState.Modified:
                    return new EmployeeUpdated(e.Id, e.Name, e.CompanyId);
                case EntityState.Added:
                    return new EmployeeCreated(e.Id, e.Name, e.CompanyId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public static object Deserialize(EntityEvent evt) => JsonConvert.DeserializeObject(evt.Payload, Type.GetType(evt.Type));
    }
}