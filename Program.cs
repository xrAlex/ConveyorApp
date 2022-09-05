using ConveyorApp.Exceptions;

namespace ConveyorApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var conveyorSectionsCount = 3;
            var conveyorOperationTimeLimit = 10000;

            var conv = new Conveyor(conveyorSectionsCount);
            var details = new List<Detail>
            {
                new Detail(),
                new Detail(),
                new Detail(),
                new Detail(),
                new Detail()
            };

            conv.AddDetails(details);


            while (true)
            {
                try
                {
                    await conv.RunProductionLine(conveyorOperationTimeLimit);
                }
                catch (ConveyorFailureException)
                {
                    Console.WriteLine("Авария но конвеере, " +
                                      "работа приостановлена, для возобновления нажмите любую кнопку");
                    Console.ReadKey();
                    await conv.Repair();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка выполнения: {ex}");
                }
            }
        }
    }
}