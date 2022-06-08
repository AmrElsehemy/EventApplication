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
        public int Price { get; set; }
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
                new Event { Name = "Phantom of the Opera", City = "New York", Price = 1 },
                new Event { Name = "Metallica", City = "Los Angeles", Price = 6 },
                new Event { Name = "Metallica", City = "New York", Price = 7 },
                new Event { Name = "Metallica", City = "Boston", Price = 9 },
                new Event { Name = "LadyGaGa", City = "New York", Price = 8 },
                new Event { Name = "LadyGaGa", City = "Boston", Price = 5 },
                new Event { Name = "LadyGaGa", City = "Chicago", Price = 3 },
                new Event { Name = "LadyGaGa", City = "San Francisco", Price = 2 },
                new Event { Name = "LadyGaGa", City = "Washington", Price = 5 }
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
            Console.WriteLine("----");

            foreach (var item in nearest5)
            {
                AddToEmail(customer, item);
            }

            // 3. If the GetDistance method is an API call which could fail or is too expensive, how will u improve the code written in 2
            var cachedDistances = new Dictionary<string, int>();

            var nearest5optimzed = events.Select(ev =>
                    {
                        if (!cachedDistances.ContainsKey(ev.City))
                        {
                            cachedDistances.Add(ev.City, GetDistance(customer.City, ev.City));
                        }

                        return new
                        {
                            Event = ev,
                            Distance = cachedDistances[ev.City]
                        };
                    }
                )
                .OrderBy(e => e.Distance)
                .Take(5)
                .Select(e => e.Event);
            Console.WriteLine("----");

            foreach (var item in nearest5optimzed)
            {
                AddToEmail(customer, item);
            }

            // 4. If the GetDistance method can fail, we don't want the process to fail. What can be done?
            // Amr: based on the business requirements here are a few solutions:
            // -- retry again with different retry policy (1 sec, 5 sec, 10 sec) 
            // -- skip the whole city and assume int.Max
            // -- since we don't really know if it's near or not, maybe an averaged distance (heuristic)

            // random value 
            var rnd = new Random((int)DateTime.Now.Ticks);

            var nearest5Failsafe = events.Select(ev =>
                    {
                        var gotDistance = rnd.Next(4) != 1;

                        if (!cachedDistances.ContainsKey(ev.City))
                        {
                            cachedDistances.Add(ev.City, GetDistance(customer.City, ev.City));
                        }

                        return new
                        {
                            Event = ev,
                            // here we decide which value to put in distance as it will affect the sort
                            Distance = gotDistance ? cachedDistances[ev.City] : int.MaxValue
                        };
                    }
                )
                .OrderBy(e => e.Distance)
                .Take(5)
                .Select(e => e.Event);

            Console.WriteLine("----");
            foreach (var item in nearest5Failsafe)
            {
                AddToEmail(customer, item);
            }

            // 5. If we also want to sort the resulting events by other fields like price, etc. to determine which
            // ones to send to the customer, how would you implement it

            // assuming they are already filtered by one of the above criteria
            var sortableEvents = events;

            // can use a lambda expression to allow many complex scenarios of sorting
            sortableEvents.Sort((x, y) => x.Price.CompareTo(y.Price));
            Console.WriteLine("----");

            foreach (var item in sortableEvents)
            {
                AddToEmail(customer, item, item.Price);
            }
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