using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using Newtonsoft.Json;
using NHibernate.Dialect;
using NHibernate.Driver;


public class User
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Username { get; set; }
    public virtual string Email { get; set; }
    public virtual Address Address { get; set; }
    public virtual string Phone { get; set; }
    public virtual string Website { get; set; }
    public virtual Company Company { get; set; }
    
    
}

public class Address
{
    public virtual string Street { get; set; }
    public virtual string Suite { get; set; }
    public virtual string City { get; set; }
    public virtual string Zipcode { get; set; }
    public virtual Geo Geo { get; set; }
}

public class Geo
{
    public virtual string Lat { get; set; }
    public virtual string Lng { get; set; }
}

public class Company
{    
    public virtual string Name { get; set; }
    public virtual string CatchPhrase { get; set; }
    public virtual string Bs { get; set; }
}



public class UserMap : ClassMap<User>
{
    public UserMap()
    {
        Table("users"); 
        Id(x => x.Id).Column("id").GeneratedBy.Native(); 
        Map(x => x.Name).Column("name");
        Map(x => x.Username).Column("username");
        Map(x => x.Email).Column("email");

        Component(x => x.Address, address =>
        {
            address.Map(x => x.Street).Column("street");
            address.Map(x => x.Suite).Column("suite");
            address.Map(x => x.City).Column("city");
            address.Map(x => x.Zipcode).Column("zipcode");

            
            address.Component(x => x.Geo, geo =>
            {
                geo.Map(x => x.Lat).Column("lat");
                geo.Map(x => x.Lng).Column("lng");
            });
        });

        
        Component(x => x.Company, company =>
        {
            company.Map(x => x.Name).Column("company_name");
            company.Map(x => x.CatchPhrase).Column("company_catchphrase");
            company.Map(x => x.Bs).Column("company_bs");
        });

        Map(x => x.Phone).Column("phone");
        Map(x => x.Website).Column("website");
    }
}

class Program
{
    static void Main()
    {        
        string connStr = "Server=localhost;Port=3306;User=root;Password=Susitha@1997;Database=restdb";
        
        var sessionFactory = Fluently.Configure()
            .Database(
                MySQLConfiguration.Standard
                    .ConnectionString(connStr)
                    .Dialect<MySQL57Dialect>()
                    .Driver<MySqlDataDriver>()
            )
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>())            
            .ExposeConfiguration(cfg => cfg.SetProperty("hibernate.show_sql", "true")) 
            .BuildSessionFactory();


        // Fetch data from the API
        List<User> users;
        string apiUrl = "https://jsonplaceholder.typicode.com/users";

        using (var httpClient = new HttpClient())
        {
            var json = httpClient.GetStringAsync(apiUrl).Result;
            users = JsonConvert.DeserializeObject<List<User>>(json);
        }

        // Insert the fetched data into the database
        using (var session = sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            foreach (var user in users)
            {
                session.Save(user);
            }

            transaction.Commit();
        }

        Console.WriteLine("Data inserted into the database.");
    }
}
