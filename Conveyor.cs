using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ConveyorApp.Enums;
using ConveyorApp.Exceptions;
using static System.Collections.Specialized.BitVector32;

namespace ConveyorApp
{
    public class Conveyor
    {
        private readonly Queue<Detail> _details = new ();
        private readonly IReadOnlyList<ConveyorSection> _sections;
        private CancellationTokenSource _cts;
        
        public Conveyor(int sectionsCount)
        {
            var sections = new List<ConveyorSection>(sectionsCount);

            for (var i = 0; i < sectionsCount; i++)
            {
                var mechanism = new Mechanism();
                var section = new ConveyorSection(mechanism, i);
                sections.Add(section);

                section.Status += SectionStatusChanged;
            }

            _sections = sections;
        }

        private void SectionStatusChanged(object? sender, SectionStatus status)
        {
            var section = (ConveyorSection)sender!;
            
            switch (status)
            {
                case SectionStatus.Empty:
                    Console.WriteLine($"Секция {section.SectionNumber} пуста");
                    break;
                case SectionStatus.Ready:
                    Console.WriteLine($"Секция {section.SectionNumber} готова к работе");
                    break;
                case SectionStatus.Working:
                    Console.WriteLine($"Секция {section.SectionNumber} в работе");
                    break;
                case SectionStatus.OperationCompleted:
                    Console.WriteLine($"Секция {section.SectionNumber} успешно завершила работу");
                    break;
                case SectionStatus.OperationFailure:
                    Console.WriteLine($"Секция {section.SectionNumber} сообщила об ошибке");
                    break;
                case SectionStatus.Stopped:
                    Console.WriteLine($"Секция {section.SectionNumber} сообщила о принудительной остнановке");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private static void Shift(ConveyorSection lastSection, ConveyorSection nextSection)
        {
            var detail = lastSection.GetDetail();
            nextSection.PutDetail(detail);
        }

        private void EmergencyStop()
        {
            _cts.Cancel();
            Console.WriteLine("Конвеер аварийно остановлен");
        }

        public async Task Repair()
        {
            foreach (var conveyorSection in _sections)
            {
                if (conveyorSection.CurrentStatus == SectionStatus.OperationFailure)
                {
                    conveyorSection.CurrentStatus = SectionStatus.Ready;
                    await conveyorSection.DoWork(_cts.Token);
                }
            }
        }

        public void AddDetails(IEnumerable<Detail> details)
        {
            foreach (var detail in details)
            {
                _details.Enqueue(detail);
            }
        }

        public async Task RunProductionLine(int operationTimeLimit)
        {
            _cts = new CancellationTokenSource();

            while (!_cts.IsCancellationRequested)
            {
                var detail = _details.Dequeue();
                var currentSectionIndex = _sections.Count - 1;
                var currentSection = _sections[currentSectionIndex];
                currentSection.PutDetail(detail);
                var operationCancellationToken = new CancellationTokenSource(operationTimeLimit);

                while (currentSectionIndex >= 0)
                {
                    try
                    {
                        await currentSection.DoWork(operationCancellationToken.Token);

                        if (currentSectionIndex == 0)
                        {
                           break;
                        }

                        if (operationCancellationToken.IsCancellationRequested)
                        {
                            EmergencyStop();
                            throw new ConveyorFailureException();
                        }
                        
                        currentSectionIndex--;
                        var nextSection = _sections[currentSectionIndex];
                        Shift(currentSection, nextSection);
                        currentSection = nextSection;
                    }
                    catch(ConveyorFailureException)
                    {
                        EmergencyStop();
                        throw;
                    }
                }
                
                _details.Enqueue(detail);
            }
        }
    }
}
