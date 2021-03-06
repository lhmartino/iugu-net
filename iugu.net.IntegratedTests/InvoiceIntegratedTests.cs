﻿using iugu.net.Entity;
using iugu.net.Filters;
using iugu.net.Lib;
using iugu.net.Request;
using iugu.net.Response;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iugu.net.Entity.Lists;

namespace iugu.net.IntegratedTests
{
    [TestFixture]
    public class InvoiceIntegratedTests
    {
        [Test]
        public async Task Create_a_valid_invoice()
        {
            // Arrange
            InvoiceModel invoice;

            var customVariables = new List<CustomVariables>
            {
                new CustomVariables { Name = "TaxaIugu", Value = "2,50" },
                new CustomVariables { Name = "TaxaPlataformaEdux", Value = "1,00" }
            };

            var invoiceDate = DateTime.Now.AddDays(2);
            var items = new List<InvoiceItem> {
                new InvoiceItem { Description = "Mensalidade", PriceCents = 100000, Quantity = 1 }
            };
            var customer = new Request.CustomerRequestMessage
            {
                Email = "anyemail@email.com",
                Name = "Client Name",
                CustomVariables = customVariables,
                CpfOrCnpj = "20250884000140", //from http://www.geradorcnpj.com/                
                zip_code = "01310940",
                number = 900,
                complement = "1º Subsolo"
            };

            // Act
            using (var apiInvoice = new Invoice())
            using (var apiCustomer = new Customer())
            using (var apiSubscription = new Subscription())
            using (var apiPlan = new Plans())
            {
                var customerResponse = await apiCustomer.CreateAsync(customer, null).ConfigureAwait(false);

                var radomPlan = Guid.NewGuid().ToString();
                PlanRequestMessage prm = new PlanRequestMessage($"{radomPlan}-12x", $"{radomPlan}-Plan", 1, PlanIntervalType.Monthly, 0, CurrencyType.BRL);

                var plan = await apiPlan.CreateAsync(prm).ConfigureAwait(false);
                //var plan = await apiPlan.CreateAsync($"{radomPlan}-12x", $"{radomPlan}-Plan", 1, "months", 0, "BRL", null, null, Constants.PaymentMethod.BANK_SLIP).ConfigureAwait(false);


                var subscriptionItems = new List<SubscriptionSubitem> { new SubscriptionSubitem { Description = "Mensalidade", PriceCents = 65000, Quantity = 1, Recurrent = true } };


                var subscription = await apiSubscription.CreateAsync(new Request.SubscriptionRequestMessage(customerResponse.ID)
                {
                    PlanId = plan.Identifier,
                    IsCreditBased = false,
                    CustomVariables = customVariables,
                    Subitems = subscriptionItems
                }).ConfigureAwait(false);

                var invoiceItems = new Item[] { new Item { Description = "Mensalidade", PriceCents = 65000, Quantity = 1 } };
                InvoiceRequestMessage irm = new InvoiceRequestMessage("anyemail@gmail.com.br", invoiceDate, invoiceItems)
                {
                    CustomerId = customerResponse.ID
                };
                invoice = await apiInvoice.CreateAsync(irm).ConfigureAwait(false);
            };

            // Assert
            Assert.That(invoice, Is.Not.Null);
            Assert.That(invoice.DueDate, Does.Contain(invoiceDate.ToString("yyyy-MM-dd")));
        }

        [Test]
        public async Task Create_a_new_invoice_and_cancel_after()
        {
            // Arrange
            InvoiceModel invoice;

            var customVariables = new List<CustomVariables>
            {
                new CustomVariables { Name = "TaxaIugu", Value = "2,50" },
                new CustomVariables { Name = "TaxaPlataformaEdux", Value = "1,00" }
            };

            var invoiceDate = DateTime.Now.AddDays(2);
            var newDate = invoiceDate.AddDays(3).ToString("dd/MM/yyyy");

            var items = new List<InvoiceItem> {
                new InvoiceItem { Description = "Mensalidade", PriceCents = 100000, Quantity = 1 }
            };

            var customer = new Request.CustomerRequestMessage
            {
                Email = "anyemail@email.com",
                Name = "Client Name",
                CustomVariables = customVariables,
                CpfOrCnpj = "20250884000140", //from http://www.geradorcnpj.com/                
                zip_code = "01310940",
                number = 900,
                complement = "1º Subsolo"
            };

            // Act
            using (var apiInvoice = new Invoice())
            using (var apiCustomer = new Customer())
            using (var apiSubscription = new Subscription())
            using (var apiPlan = new Plans())
            {
                var customerResponse = await apiCustomer.CreateAsync(customer, null).ConfigureAwait(false);
                var radomPlan = Guid.NewGuid().ToString();
                PlanRequestMessage prm = new PlanRequestMessage($"{radomPlan}-12x", $"{radomPlan}-Plan", 1, PlanIntervalType.Monthly, 0, CurrencyType.BRL);
                var plan = await apiPlan.CreateAsync(prm).ConfigureAwait(false);
                var subscriptionItems = new List<SubscriptionSubitem> { new SubscriptionSubitem { Description = "Mensalidade", PriceCents = 65000, Quantity = 1, Recurrent = true } };
                var subscription = await apiSubscription.CreateAsync(new Request.SubscriptionRequestMessage(customerResponse.ID)
                {
                    PlanId = plan.Identifier,                    
                    IsCreditBased = false,
                    CustomerId = customerResponse.ID, 
                    CustomVariables = customVariables,
                    Subitems = subscriptionItems                    
                }).ConfigureAwait(false);

                var invoiceItems = new Item[] { new Item { Description = "Mensalidade", PriceCents = 65000, Quantity = 1 } };
                InvoiceRequestMessage irm = new InvoiceRequestMessage("anyemail@gmail.com.br", invoiceDate, invoiceItems)
                {
                    CustomerId = customerResponse.ID
                };
                var current = await apiInvoice.CreateAsync(irm);

                invoice = await apiInvoice.DuplicateAsync(current.ID, new Request.InvoiceDuplicateRequestMessage(newDate)).ConfigureAwait(false);
            };

            // Assert
            Assert.That(invoice, Is.Not.Null);
        }

        [Test]
        public async Task Create_a_new_invoice_with_custom_api_token()
        {
            // Arrange
            InvoiceModel invoice;

            var customVariables = new List<CustomVariables>
            {
                new CustomVariables { Name = "TaxaIugu", Value = "2,50" },
                new CustomVariables { Name = "TaxaPlataformaEdux", Value = "1,00" }
            };

            var invoiceDate = DateTime.Now.AddDays(2);
            var newDate = invoiceDate.AddDays(3).ToString("dd/MM/yyyy");

            var items = new List<InvoiceItem> {
                new InvoiceItem { Description = "Mensalidade", PriceCents = 100000, Quantity = 1 }
            };
            var customer = new Request.CustomerRequestMessage
            {
                Email = "anyemail@email.com",
                Name = "Client Name",
                CustomVariables = customVariables,
                CpfOrCnpj = "20250884000140", //from http://www.geradorcnpj.com/                
                zip_code = "01310940",
                number = 900,
                complement = "1º Subsolo"
            };

            // Act
            using (var apiInvoice = new Invoice())
            using (var apiCustomer = new Customer())
            using (var apiSubscription = new Subscription())
            using (var apiPlan = new Plans())
            {
                var customerResponse = await apiCustomer.CreateAsync(customer, null).ConfigureAwait(false);
                var radomPlan = Guid.NewGuid().ToString();
                PlanRequestMessage prm = new PlanRequestMessage($"{radomPlan}-12x", $"{radomPlan}-Plan", 1, PlanIntervalType.Monthly, 0, CurrencyType.BRL);
                var plan = await apiPlan.CreateAsync(prm).ConfigureAwait(false);
                var subscriptionItems = new List<SubscriptionSubitem> { new SubscriptionSubitem { Description = "Mensalidade", PriceCents = 65000, Quantity = 1, Recurrent = true } };
                var subscription = await apiSubscription.CreateAsync(new Request.SubscriptionRequestMessage(customerResponse.ID)
                {
                    PlanId = plan.Identifier,
                    IsCreditBased = false,
                    CustomVariables = customVariables,
                    Subitems = subscriptionItems
                }).ConfigureAwait(false);

                var invoiceItems = new Item[] { new Item { Description = "Mensalidade", PriceCents = 65000, Quantity = 1 } };
                var invoiceRequest = new InvoiceRequestMessage("anyemail@gmail.com.br", invoiceDate, invoiceItems)
                {
                    SubscriptionId = subscription.ID,
                    CustomerId = customerResponse.ID,
                    CustomVariables = customVariables.ToArray(),
                };

                invoice = await apiInvoice.CreateAsync(invoiceRequest, "2e90be2d765e374bed509f2d7676a921").ConfigureAwait(false);//aaaa
            };

            // Assert
            Assert.That(invoice, Is.Not.Null);
        }

        [Test]
        public async Task Create_a_new_invoice_with_custom_api_token_and_cancel_after()
        {
            // Arrange
            const string customApiToken = "2e90be2d765e374bed509f2d7676a921";
            InvoiceModel invoice;
            InvoiceModel cancelInvoice;

            var customVariables = new List<CustomVariables>
            {
                new CustomVariables { Name = "TaxaIugu", Value = "2,50" },
                new CustomVariables { Name = "TaxaPlataformaEdux", Value = "1,00" }
            };

            var invoiceDate = DateTime.Now.AddDays(2);
            var newDate = invoiceDate.AddDays(3).ToString("dd/MM/yyyy");

            var items = new List<InvoiceItem> {
                new InvoiceItem { Description = "Mensalidade", PriceCents = 100000, Quantity = 1 }
            };

            var customer = new Request.CustomerRequestMessage
            {
                Email = "anyemail@email.com",
                Name = "Client Name",
                CustomVariables = customVariables
            };

            // Act
            using (var apiInvoice = new Invoice())
            using (var apiCustomer = new Customer())
            using (var apiSubscription = new Subscription())
            using (var apiPlan = new Plans())
            {
                var customerResponse = await apiCustomer.CreateAsync(customer, null).ConfigureAwait(false);
                var radomPlan = Guid.NewGuid().ToString();
                PlanRequestMessage prm = new PlanRequestMessage($"{radomPlan}-12x", $"{radomPlan}-Plan", 1, PlanIntervalType.Monthly, 0, CurrencyType.BRL);
                var plan = await apiPlan.CreateAsync(prm).ConfigureAwait(false);
                var subscriptionItems = new List<SubscriptionSubitem> { new SubscriptionSubitem { Description = "Mensalidade", PriceCents = 65000, Quantity = 1, Recurrent = true } };
                var subscription = await apiSubscription.CreateAsync(new SubscriptionRequestMessage(customerResponse.ID)
                {
                    PlanId = plan.Identifier,
                    IsCreditBased = false,
                    CustomVariables = customVariables,
                    Subitems = subscriptionItems
                }, customApiToken).ConfigureAwait(false);

                var invoiceItems = new Item[] { new Item { Description = "Mensalidade", PriceCents = 65000, Quantity = 1 } };
                var request = new InvoiceRequestMessage("anyemail@gmail.com.br", invoiceDate, invoiceItems)
                {
                    SubscriptionId = subscription.ID,
                    CustomVariables = customVariables.ToArray(),
                    Payer = new PayerModel
                    {
                        Address = new AddressModel
                        {
                            City = "São Paulo",
                            Country = "Brasil",
                            State = "São Paulo",
                            Number = "100",
                            Street = "Any Street",
                            ZipCode = "01310940"
                        },
                        CpfOrCnpj = "20250884000140", //from http://www.geradorcnpj.com/        
                        Name = "Cliente da IUGU",
                        PhonePrefix = "99",
                        Phone = "9999-9999",
                    },
                    EnableLateFine = true,
                    LatePaymentFine = "2%",
                    EnableProportionalDailyTax = true,
                    PaymentMethod = Constants.PaymentMethod.BANK_SLIP,

                };

                var current = await apiInvoice.CreateAsync(request, customApiToken).ConfigureAwait(false);
                invoice = await apiInvoice.DuplicateAsync(current.ID, new InvoiceDuplicateRequestMessage(newDate), customApiToken).ConfigureAwait(false);
                cancelInvoice = await apiInvoice.GetAsync(current.ID, customApiToken).ConfigureAwait(false);
            };

            // Assert
            Assert.That(invoice, Is.Not.Null);
            Assert.That(invoice.Status, Is.EqualTo(Constants.InvoiceStatus.PENDING));
            Assert.That(cancelInvoice.Status, Is.EqualTo(Constants.InvoiceStatus.CANCELED));
        }

        [Test]
        public async Task List_invoices()
        {
            // Arrange
            InvoicesModel invoices = null;

            // Act
            using (var apiInvoice = new Invoice())
            {
                invoices = await apiInvoice.GetAsync().ConfigureAwait(false);
            };

            // Assert
            Assert.That(invoices, Is.Not.Null);
            Assert.That(invoices?.Items, Is.Not.Empty);
        }

        [Test]
        public async Task Resend_invoice_mail()
        {
            // Arrange
            InvoicesModel invoices = null;
            InvoiceModel resendInvoiceModel = null;
            var resendInvoiceId = "";

            // Act
            using (var apiInvoice = new Invoice())
            {
                invoices = await apiInvoice.GetAsync().ConfigureAwait(false);
                resendInvoiceId = invoices.Items.First().ID;
                resendInvoiceModel = await apiInvoice.ResendInvoiceMail(resendInvoiceId).ConfigureAwait(false);
            };

            // Assert
            Assert.That(resendInvoiceModel, Is.Not.Null);
            Assert.That(resendInvoiceModel.ID, Is.EqualTo(resendInvoiceId));
        }

        [Test]
        public async Task Get_all_invoices_pagged_by_custom_api_token()
        {
            // Arrange
            PaggedResponseMessage<InvoiceModel> response;
            var filter = new QueryStringFilter
            {
                MaxResults = 1000,
            };

            // Act
            using (var apiInvoice = new Invoice())
            {
                response = await apiInvoice.GetAllAsync("2e90be2d765e374bed509f2d7676a921", filter).ConfigureAwait(false);
            };

            // Assert
            Assert.That(response.Items, Is.Not.Empty);
            Assert.That(response.TotalItems, Is.GreaterThan(0));
        }

    }
}
