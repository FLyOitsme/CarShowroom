using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DataLayer.Entities;
using System.Globalization;
using CarShowroom.Interfaces;

namespace CarShowroom.Services
{
    public class PdfContractService : IPdfContractService
    {
        public byte[] GenerateContract(
            Sale sale,
            Client client,
            User manager,
            Car car,
            List<Addition> additions,
            List<Discount> discounts,
            decimal basePrice,
            decimal finalPrice)
        {
            // Устанавливаем лицензию (Community - бесплатная)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header()
                        .AlignCenter()
                        .PaddingBottom(10)
                        .Text("ДОГОВОР КУПЛИ-ПРОДАЖИ АВТОМОБИЛЯ")
                        .FontSize(18)
                        .Bold();

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Дата и номер договора
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem().Text($"Дата: {sale.Date:dd.MM.yyyy}");
                                    row.RelativeItem().AlignRight().Text($"№ {sale.Id}");
                                });

                            column.Item().PaddingTop(10).Text(text =>
                            {
                                text.Span(DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"))).FontSize(12); 
                                text.Span("г. ").FontSize(12);
                            });

                            // Стороны договора
                            column.Item().PaddingTop(10).Text("ПРОДАВЕЦ:").Bold();
                            column.Item().PaddingLeft(20).Text(text =>
                            {
                                text.Span("ООО \"Автосалон\"\n");
                                text.Span("ИНН: 1234567890\n");
                                text.Span("Адрес: г. Москва, ул. Примерная, д. 1");
                            });

                            column.Item().PaddingTop(10).Text("ПОКУПАТЕЛЬ:").Bold();
                            column.Item().PaddingLeft(20).Text(text =>
                            {
                                text.Span($"{client.Surname} {client.Name} {client.Patronyc}\n");
                                if (!string.IsNullOrEmpty(client.PhoneNumber))
                                    text.Span($"Телефон: {client.PhoneNumber}\n");
                                if (!string.IsNullOrEmpty(client.PassData))
                                    text.Span($"Паспортные данные: {client.PassData}\n");
                            });

                            // Предмет договора
                            column.Item().PaddingTop(15).Text("1. ПРЕДМЕТ ДОГОВОРА").Bold();
                            column.Item().PaddingLeft(20).Text(text =>
                            {
                                text.Span("Продавец обязуется передать в собственность Покупателя, а Покупатель обязуется принять и оплатить следующий автомобиль:\n\n");
                                
                                text.Span("Марка: ").Bold();
                                text.Span($"{car.Model?.Brand?.Name ?? "Неизвестно"}\n");
                                
                                text.Span("Модель: ").Bold();
                                text.Span($"{car.Model?.Name ?? "Неизвестно"}\n");
                                
                                text.Span("Год выпуска: ").Bold();
                                text.Span($"{car.Year}\n");
                                
                                text.Span("Цвет: ").Bold();
                                text.Span($"{car.Color ?? "Не указан"}\n");
                                
                                text.Span("Объем двигателя: ").Bold();
                                text.Span($"{car.EngVol:F1} л\n");
                                
                                text.Span("Пробег: ").Bold();
                                text.Span($"{car.Mileage:N0} км\n");
                                
                                text.Span("Коробка передач: ").Bold();
                                text.Span($"{car.Transmission?.Name ?? "Не указана"}\n");
                                
                                text.Span("Тип кузова: ").Bold();
                                text.Span($"{car.Type?.Name ?? "Не указан"}\n");
                                
                                text.Span("Состояние: ").Bold();
                                text.Span($"{car.Condition?.Name ?? "Не указано"}\n");
                            });

                            // Дополнительные опции
                            if (additions.Any())
                            {
                                column.Item().PaddingTop(10).Text("Дополнительные опции:").Bold();
                                foreach (var addition in additions)
                                {
                                    column.Item().PaddingLeft(20).Text($"- {addition.Name}: {addition.Cost:N0} ₽");
                                }
                            }

                            // Скидки
                            if (discounts.Any())
                            {
                                column.Item().PaddingTop(10).Text("Примененные скидки:").Bold();
                                foreach (var discount in discounts)
                                {
                                    column.Item().PaddingLeft(20).Text($"- {discount.Name}: {discount.Cost}%");
                                }
                            }

                            // Цена
                            column.Item().PaddingTop(15).Text("2. ЦЕНА И ПОРЯДОК РАСЧЕТОВ").Bold();
                            column.Item().PaddingLeft(20).Text(text =>
                            {
                                text.Span("Базовая цена автомобиля: ").Bold();
                                text.Span($"{basePrice:N0} ₽\n");
                                
                                if (additions.Any())
                                {
                                    var additionsCost = additions.Sum(a => (decimal)(a.Cost ?? 0));
                                    text.Span("Стоимость дополнительных опций: ").Bold();
                                    text.Span($"{additionsCost:N0} ₽\n");
                                }
                                
                                text.Span("Итоговая цена: ").Bold().FontSize(14);
                                text.Span($"{finalPrice:N0} ₽").FontSize(14);
                            });

                            // Ответственность сторон
                            column.Item().PaddingTop(15).Text("3. ОТВЕТСТВЕННОСТЬ СТОРОН").Bold();
                            column.Item().PaddingLeft(20).Text("Стороны несут ответственность за неисполнение или ненадлежащее исполнение обязательств по настоящему договору в соответствии с законодательством Российской Федерации.");

                            // Подписи
                            column.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("ПРОДАВЕЦ:").Bold();
                                    column.Item().PaddingTop(40).Text("_________________");
                                    column.Item().Text($"{manager.Surname} {manager.Name} {manager.Patronyc}");
                                });
                                
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("ПОКУПАТЕЛЬ:").Bold();
                                    column.Item().PaddingTop(40).Text("_________________");
                                    column.Item().Text($"{client.Surname} {client.Name} {client.Patronyc}");
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
