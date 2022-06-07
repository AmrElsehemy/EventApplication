using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Viagogo
{
    public class Event
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class Solution
    {
        static void Main(string[] args)
        {
            var events = new List<Event>
            {
                new Event { Name = "Phantom of the Opera", City = "New York" },
                new Event { Name = "Metallica", City = "Los Angeles" },
                new Event { Name = "Metallica", City = "New York" },
                new Event { Name = "Metallica", City = "Boston" },
                new Event { Name = "LadyGaGa", City = "New York" },
                new Event { Name = "LadyGaGa", City = "Boston" },
                new Event { Name = "LadyGaGa", City = "Chicago" },
                new Event { Name = "LadyGaGa", City = "San Francisco" },
                new Event { Name = "LadyGaGa", City = "Washington" }
            };

            // pre-req: grouping data structure for faster access
            var groupedEventsByCity =
                from evt in events
                group evt by evt.City;

            // 1. find out all events that are in cities of customer
            // then add to email.
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            var customerEvents = groupedEventsByCity.FirstOrDefault(p => p.Key == customer.City);
            foreach (var item in customerEvents?.ToList())
            {
                AddToEmail(customer, item);
            }

            // 2. nearest5
            var nearest5 = events.Select(ev =>
                    new { Event = ev, Distance = GetDistance(customer.City, ev.City) })
                .OrderBy(e => e.Distance)
                .Take(5)
                .Select(e => e.Event);

            foreach (var item in nearest5)
            {
                AddToEmail(customer, item);
            }

            // 3. If the GetDistance method is an API call which could fail or is too expensive, how will u improve the code written in 2
            var cachedDistances = new Dictionary<string, int>();
            foreach (var evt in events.Where(evt => !cachedDistances.ContainsKey(evt.City)))
            {
                cachedDistances.Add(evt.City, GetDistance(customer.City, evt.City));
            }

            var nearest5optimzed = events.Select(ev =>
                    new { Event = ev, Distance = cachedDistances[ev.City] })
                .OrderBy(e => e.Distance)
                .Take(5)
                .Select(e => e.Event);

            foreach (var item in nearest5optimzed)
            {
                AddToEmail(customer, item);
            }
            
            // 4. If the GetDistance method can fail, we don't want the process to fail. What can be done?
            // Amr: based on the business requirements here are a few solutions:
             // -- retry again with different retry policy (1 sec, 5 sec, 10 sec) 
             // -- skip the whole city and assume int.Max
             // -- since we don't really know if it's near or not, maybe an averaged distance (heuristic)
            
        }

        // You do not need to know how these methods work

        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
                                  + (distance > 0 ? $" ({distance} miles away)" : "")
                                  + (price.HasValue ? $" for ${price}" : ""));
        }

        static int GetPrice(Event e)
        {
            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }

        static int GetDistance(string fromCity, string toCity)
        {
            return AlphebiticalDistance(fromCity, toCity);
        }

        private static int AlphebiticalDistance(string s, string t)
        {
            var result = 0;
            var i = 0;
            for (i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 1 i={i} {s.Length} {t.Length}");
                result += Math.Abs(s[i] - t[i]);
            }

            for (; i < Math.Max(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 2 i={i} {s.Length} {t.Length}");
                result += s.Length > t.Length ? s[i] : t[i];
            }

            return result;
        }
    }
}