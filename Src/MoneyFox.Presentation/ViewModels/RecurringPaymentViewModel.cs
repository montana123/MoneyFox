﻿using System;
using GenericServices;
using MoneyFox.DataLayer.Entities;
using MoneyFox.Foundation;

namespace MoneyFox.Presentation.ViewModels
{
    public class RecurringPaymentViewModel : BaseViewModel, ILinkToEntity<RecurringPayment>
    {
        private int id;
        private DateTime startDate;
        private DateTime? endDate;
        private double amount;
        private bool isEndless;
        private PaymentType type;
        private PaymentRecurrence recurrence;
        private string note;

        private AccountViewModel chargedAccount;
        private CategoryViewModel categoryViewModel;

        public RecurringPaymentViewModel()
        {
            Recurrence = PaymentRecurrence.Daily;
            EndDate = DateTime.Today;
        }

        public int Id
        {
            get => id;
            set
            {
                if (id == value) return;
                id = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                if (startDate == value) return;
                startDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                if (endDate == value) return;
                endDate = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEndless
        {
            get => isEndless;
            set
            {
                if (isEndless == value) return;
                isEndless = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Amount of the payment. Has to be >= 0. If the amount is charged or not is based on the payment type.
        /// </summary>
        public double Amount
        {
            get => amount;
            set
            {
                if (Math.Abs(amount - value) < 0.01) return;
                amount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Type of the payment <see cref="PaymentType" />.
        /// </summary>
        public PaymentType Type
        {
            get => type;
            set
            {
                if (type == value) return;
                type = value;
                RaisePropertyChanged();
            }
        }

        public PaymentRecurrence Recurrence
        {
            get => recurrence;
            set
            {
                if (recurrence == value) return;
                recurrence = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Additional notes to the payment.
        /// </summary>
        public string Note
        {
            get => note;
            set
            {
                if (note == value) return;
                note = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     In case it's a expense or transfer the account who will be charged.
        ///     In case it's an income the account who will be credited.
        /// </summary>
        public AccountViewModel ChargedAccount
        {
            get => chargedAccount;
            set
            {
                if(chargedAccount == value) return;
                chargedAccount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     The <see cref="Category" /> for this payment
        /// </summary>
        public CategoryViewModel Category
        {
            get => categoryViewModel;
            set
            {
                if (categoryViewModel == value) return;
                categoryViewModel = value;
                RaisePropertyChanged();
            }
        }
    }
}