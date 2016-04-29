﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyFox.Shared.Exceptions;
using MoneyFox.Shared.Helpers;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.Repositories;
using MoneyFox.Shared.Tests.Mocks;
using Moq;
using MvvmCross.Platform;
using MvvmCross.Test.Core;
using Assert = Xunit.Assert;

namespace MoneyFox.Shared.Tests.Repositories
{
    [TestClass]
    public class PaymentRepositoryTests : MvxIoCSupportingTest
    {
        private DateTime _localDateSetting;

        [TestInitialize]
        public void Init()
        {
            Setup();

            // We setup the static setting classes here for the general usage in the app
            var settingsMockSetup = new Mock<ILocalSettings>();
            settingsMockSetup.SetupAllProperties();
            settingsMockSetup.Setup(x => x.AddOrUpdateValue(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((string key, DateTime date) => _localDateSetting = date);

            var roamSettingsMockSetup = new Mock<IRoamingSettings>();
            roamSettingsMockSetup.SetupAllProperties();

            Mvx.RegisterType(() => settingsMockSetup.Object);
            Mvx.RegisterType(() => roamSettingsMockSetup.Object);
        }

        [TestMethod]
        public void SaveWithouthAccount_NoAccount_InvalidDataException()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var payment = new Payment
            {
                Amount = 20
            };

            Assert.Throws<AccountMissingException>(() => repository.Save(payment));
        }

        [TestMethod]
        public void Save_DifferentPaymentTypes_CorrectlySaved()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var paymentDataAccessMock = new PaymentDataAccessMock();
            var repository = new PaymentRepository(paymentDataAccessMock,
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            var payment = new Payment
            {
                ChargedAccount = account,
                TargetAccount = null,
                Amount = 20,
                Type = (int) PaymentType.Income
            };

            repository.Save(payment);

            Assert.Same(payment, paymentDataAccessMock.PaymentTestList[0]);
            Assert.Equal((int) PaymentType.Income, paymentDataAccessMock.PaymentTestList[0].Type);
        }

        [TestMethod]
        public void Save_TransferPayment_CorrectlySaved()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var paymentDataAccessMock = new PaymentDataAccessMock();
            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            var targetAccount = new Account
            {
                Id = 3,
                Name = "targetAccount"
            };

            var payment = new Payment
            {
                ChargedAccount = account,
                ChargedAccountId = 2,
                TargetAccount = targetAccount,
                TargetAccountId = 3,
                Amount = 20,
                Type = (int) PaymentType.Transfer
            };

            repository.Save(payment);

            Assert.Same(payment, repository.Data[0]);
            Assert.Equal((int) PaymentType.Transfer, repository.Data[0].Type);
        }

        [TestMethod]
        public void PaymentRepository_Delete()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var paymentDataAccessMock = new PaymentDataAccessMock();
            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            var payment = new Payment
            {
                ChargedAccount = account,
                ChargedAccountId = 2,
                Amount = 20
            };

            repository.Save(payment);
            Assert.Same(payment, repository.Data[0]);

            repository.Delete(payment);

            Assert.False(repository.Data.Any());
        }

        [TestMethod]
        public void PaymentRepository_AccessCache()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryRepositorySetup = new Mock<IRepository<Category>>();
            categoryRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var paymentRepo = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryRepositorySetup.Object,
                new Mock<INotificationService>().Object);

            Assert.False(paymentRepo.Data.Any());
        }

        [TestMethod]
        public void AddItemToDataList_SaveAccount_IsAddedToData()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            var payment = new Payment
            {
                ChargedAccount = account,
                Amount = 20,
                Type = (int) PaymentType.Transfer
            };

            repository.Save(payment);
            Assert.True(repository.Data.Contains(payment));
        }

        [TestMethod]
        public void GetUnclearedPayments_PastDate_PastPayments()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            repository.Data = new ObservableCollection<Payment>(new List<Payment>
            {
                new Payment
                {
                    ChargedAccount = account,
                    Amount = 55,
                    Date = DateTime.Today.AddDays(-1),
                    Note = "this is a note!!!",
                    IsCleared = false
                }
            });

            var payments = repository.GetUnclearedPayments();

            Assert.Equal(1, payments.Count());
        }

        /// <summary>
        ///     This Test may fail if the date overlaps with the month transition.
        /// </summary>
        [TestMethod]
        public void GetUnclearedPayments_FutureDate_PastPayments()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account = new Account
            {
                Id = 2,
                Name = "TestAccount"
            };

            repository.Save(new Payment
            {
                ChargedAccount = account,
                Amount = 55,
                Date = Utilities.GetEndOfMonth().AddDays(-1),
                Note = "this is a note!!!",
                IsCleared = false
            }
                );

            var payments = repository.GetUnclearedPayments();
            Assert.Equal(0, payments.Count());

            payments = repository.GetUnclearedPayments(Utilities.GetEndOfMonth());
            Assert.Equal(1, payments.Count());
        }

        [TestMethod]
        public void GetUnclearedPayments_AccountNull()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repository = new PaymentRepository(new PaymentDataAccessMock(),
                new RecurringPaymentDataAccessMock(),
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            repository.Data.Add(new Payment
            {
                Amount = 55,
                Date = DateTime.Today.AddDays(-1),
                Note = "this is a note!!!",
                IsCleared = false
            }
                );

            var payments = repository.GetUnclearedPayments();
            Assert.Equal(1, payments.Count());
        }

        [TestMethod]
        public void Load_Payment_DataInitialized()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var dataAccessSetup = new Mock<IDataAccess<Payment>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment>
            {
                new Payment {Id = 10},
                new Payment {Id = 15}
            });

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());


            var paymentRepository = new PaymentRepository(dataAccessSetup.Object,
                new Mock<IDataAccess<RecurringPayment>>().Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);
            paymentRepository.Load();

            Assert.True(paymentRepository.Data.Any(x => x.Id == 10));
            Assert.True(paymentRepository.Data.Any(x => x.Id == 15));
        }

        [TestMethod]
        public void GetRelatedPayments_Account_CorrectAccounts()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var dataAccessSetup = new Mock<IDataAccess<Payment>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repo = new PaymentRepository(dataAccessSetup.Object,
                new Mock<IDataAccess<RecurringPayment>>().Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object);

            var account1 = new Account {Id = 1};
            var account3 = new Account {Id = 3};

            repo.Data = new ObservableCollection<Payment>(new List<Payment>
            {
                new Payment {Id = 2, ChargedAccount = account1, ChargedAccountId = account1.Id},
                new Payment {Id = 5, ChargedAccount = account3, ChargedAccountId = account3.Id}
            });

            var result = repo.GetRelatedPayments(account1).ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result.First().Id);
        }

        [TestMethod]
        public void LoadRecurringList_NoParameters_ListWithRecurringTrans()
        {
            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account>());

            var dataAccessSetup = new Mock<IDataAccess<Payment>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment>());

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            var repo = new PaymentRepository(dataAccessSetup.Object,
                new Mock<IDataAccess<RecurringPayment>>().Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object)
            {
                Data = new ObservableCollection<Payment>(new List<Payment>
                {
                    new Payment
                    {
                        Id = 1,
                        IsRecurring = true,
                        RecurringPayment = new RecurringPayment {Id = 1, IsEndless = true},
                        RecurringPaymentId = 1
                    },
                    new Payment {Id = 2, IsRecurring = false},
                    new Payment
                    {
                        Id = 3,
                        IsRecurring = true,
                        RecurringPayment =
                            new RecurringPayment {Id = 2, IsEndless = false, EndDate = DateTime.Today.AddDays(10)},
                        RecurringPaymentId = 2
                    },
                    new Payment
                    {
                        Id = 4,
                        IsRecurring = true,
                        RecurringPayment =
                            new RecurringPayment {Id = 3, IsEndless = false, EndDate = DateTime.Today.AddDays(-10)},
                        RecurringPaymentId = 3
                    }
                })
            };

            var result = repo.LoadRecurringList().ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(3, result[1].Id);
        }

        [TestMethod]
        public void DeletePayment_IncomeCleared_DeletedAccountBalanceSet()
        {
            var deletedId = 0;

            var account = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account.Id,
                ChargedAccount = account,
                Amount = 50,
                Type = (int) PaymentType.Income,
                IsCleared = true
            };

            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object)
                .Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account.CurrentBalance);
        }

        [TestMethod]
        public void DeletePayment_IncomeNotCleared_DeletedAccountBalanceSet()
        {
            var deletedId = 0;

            var account = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account.Id,
                ChargedAccount = account,
                Amount = 50,
                Type = (int) PaymentType.Income,
                IsCleared = false
            };

            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object)
                .Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account.CurrentBalance);
        }

        [TestMethod]
        public void DeletePayment_ExpenseNotCleared_DeletedAccountBalanceSet()
        {
            var deletedId = 0;

            var account = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account.Id,
                ChargedAccount = account,
                Amount = 50,
                Type = (int) PaymentType.Expense,
                IsCleared = false
            };

            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object)
                .Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account.CurrentBalance);
        }

        [TestMethod]
        public void DeletePayment_ExpenseCleared_DeletedAccountBalanceSet()
        {
            var deletedId = 0;

            var account = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account.Id,
                ChargedAccount = account,
                Amount = 50,
                Type = (int) PaymentType.Expense,
                IsCleared = true
            };

            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object)
                .Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account.CurrentBalance);
        }

        [TestMethod]
        public void DeletePayment_TransferClearedTrue_Deleted()
        {
            var deletedId = 0;

            var account1 = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };
            var account2 = new Account
            {
                Id = 4,
                Name = "just an account",
                CurrentBalance = 900
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account1.Id,
                ChargedAccount = account1,
                TargetAccountId = account2.Id,
                TargetAccount = account2,
                Amount = 50,
                Type = (int) PaymentType.Transfer,
                IsCleared = true
            };


            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account1, account2});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object).Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account1.CurrentBalance);
            Assert.Equal(900, account2.CurrentBalance);
        }

        [TestMethod]
        public void DeletePayment_TransferClearedFalse_Deleted()
        {
            var deletedId = 0;

            var account1 = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };
            var account2 = new Account
            {
                Id = 4,
                Name = "just an account",
                CurrentBalance = 900
            };

            var payment = new Payment
            {
                Id = 10,
                ChargedAccountId = account1.Id,
                ChargedAccount = account1,
                TargetAccountId = account2.Id,
                TargetAccount = account2,
                Amount = 50,
                Type = (int) PaymentType.Transfer,
                IsCleared = false
            };


            var paymentDataAccessMockSetup = new Mock<IDataAccess<Payment>>();
            paymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<Payment>()))
                .Callback((Payment trans) => deletedId = trans.Id);
            paymentDataAccessMockSetup.Setup(x => x.SaveItem(It.IsAny<Payment>()));
            paymentDataAccessMockSetup.Setup(x => x.LoadList(null)).Returns(new List<Payment> {payment});

            var recPaymentDataAccessMockSetup = new Mock<IDataAccess<RecurringPayment>>();
            recPaymentDataAccessMockSetup.Setup(x => x.DeleteItem(It.IsAny<RecurringPayment>()));
            recPaymentDataAccessMockSetup.Setup(x => x.LoadList(It.IsAny<Expression<Func<RecurringPayment, bool>>>()))
                .Returns(new List<RecurringPayment>());

            var accountRepositorySetup = new Mock<IAccountRepository>();
            accountRepositorySetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Account> {account1, account2});

            var categoryDataAccessSetup = new Mock<IRepository<Category>>();
            categoryDataAccessSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>());

            new PaymentRepository(paymentDataAccessMockSetup.Object,
                recPaymentDataAccessMockSetup.Object,
                accountRepositorySetup.Object,
                categoryDataAccessSetup.Object,
                new Mock<INotificationService>().Object).Delete(
                    payment);

            Assert.Equal(10, deletedId);
            Assert.Equal(500, account1.CurrentBalance);
            Assert.Equal(900, account2.CurrentBalance);
        }

        [TestMethod]
        public void Save_UpdateTimeStamp()
        {
            var dataAccessSetup = new Mock<IDataAccess<Category>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Category>());

            new CategoryRepository(dataAccessSetup.Object,
                new Mock<INotificationService>().Object)
                .Save(new Category());
            _localDateSetting.ShouldBeGreaterThan(DateTime.Now.AddSeconds(-1));
            _localDateSetting.ShouldBeLessThan(DateTime.Now.AddSeconds(1));
        }
    }
}