using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ConveyorApp.Enums;
using ConveyorApp.Exceptions;

namespace ConveyorApp
{
    internal sealed class ConveyorSection
    {
        private readonly Mechanism _mechanism;
        private Detail _detail;
        private SectionStatus _currentStatus = SectionStatus.Empty;
        public int SectionNumber { get; }
        public SectionStatus CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                Status.Invoke(this, _currentStatus);
            }
        }

        public event EventHandler<SectionStatus> Status;

        public ConveyorSection(Mechanism mechanism, int sectionNumber)
        {
            _mechanism = mechanism;
            SectionNumber = sectionNumber;
        }

        public void PutDetail(Detail detail)
        {
            _detail = detail;
            CurrentStatus = SectionStatus.Ready;
        }

        public async Task DoWork(CancellationToken cts)
        {
            CurrentStatus = SectionStatus.Working;

            if (cts.IsCancellationRequested)
            {
                CurrentStatus = SectionStatus.Stopped;
            }

            try
            {
                await _mechanism.DoWork(cts);
                CurrentStatus = SectionStatus.OperationCompleted;
            }
            catch (ConveyorFailureException)
            {
                CurrentStatus = SectionStatus.OperationFailure;
                throw;
            }
            catch (TaskCanceledException)
            {
                CurrentStatus = SectionStatus.Stopped;
            }
        }
        
        public Detail GetDetail()
        {
            CurrentStatus = SectionStatus.Empty;
            return _detail;
        }
    }
}
