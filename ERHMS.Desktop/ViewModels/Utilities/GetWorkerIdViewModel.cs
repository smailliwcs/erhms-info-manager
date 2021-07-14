﻿using ERHMS.Desktop.ViewModels.Collections;
using ERHMS.EpiInfo.Data;
using System.Threading.Tasks;

namespace ERHMS.Desktop.ViewModels.Utilities
{
    public class GetWorkerIdViewModel
    {
        public static async Task<GetWorkerIdViewModel> CreateAsync(
            string firstName,
            string lastName,
            string emailAddress)
        {
            GetWorkerIdViewModel result = new GetWorkerIdViewModel(firstName, lastName, emailAddress);
            await result.InitializeAsync();
            return result;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string EmailAddress { get; }
        public WorkerCollectionViewModel Workers { get; private set; }

        public string WorkerId
        {
            get
            {
                return Workers.CurrentItem?.GlobalRecordId;
            }
            set
            {
                Workers.Items.MoveCurrentTo(
                    worker => Record.GlobalRecordIdComparer.Equals(worker.GlobalRecordId, value));
            }
        }

        private GetWorkerIdViewModel(string firstName, string lastName, string emailAddress)
        {
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
        }

        private async Task InitializeAsync()
        {
            Workers = await WorkerCollectionViewModel.CreateAsync(FirstName, LastName, EmailAddress);
        }
    }
}
