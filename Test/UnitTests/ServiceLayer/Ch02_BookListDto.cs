﻿// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.BookServices;
using ServiceLayer.BookServices.QueryObjects;
using test.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace test.UnitTests.ServiceLayer
{
    public class Ch02_BookListDto
    {

        private readonly ITestOutputHelper _output;

        public Ch02_BookListDto(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestEagerBookListDtoOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var firstBook = context.Books
                    .Include(r => r.AuthorsLink)    
                         .ThenInclude(r => r.Author)

                    .Include(r => r.Reviews)        
                    .Include(r => r.Promotion)      
                    .First();
                var dto = new BookListDto
                {
                    BookId = firstBook.BookId,
                    Title = firstBook.Title,
                    Price = firstBook.Price,
                    ActualPrice = firstBook                //#A
                        .Promotion?.NewPrice               //#A
                            ?? firstBook.Price,            //#A
                    PromotionPromotionalText = firstBook   //#A
                        .Promotion?.PromotionalText,       //#A
                    AuthorsOrdered = string.Join(", ",      //#B
                         firstBook.AuthorsLink             //#B
                        .OrderBy(l => l.Order)             //#B
                        .Select(a => a.Author.Name)),      //#B
                    ReviewsCount = firstBook.Reviews.Count,//#C
                    ReviewsAverageVotes =                  //#D
                        firstBook.Reviews.Count == 0       //#D
                        ? null                             //#D
                        : (decimal?) firstBook             //#D
                              .Reviews                     //#D
                              .Average(p => p.NumStars)    //#D
                };

                //VERIFY
                dto.BookId.ShouldNotEqual(0);
                dto.Title.ShouldNotBeNull();
                dto.Price.ShouldNotEqual(0);
                dto.AuthorsOrdered.ShouldNotBeNull();
                /*********************************************************
                #A Notice the use of ?. This returns null if Promotion is null, otherwise it returns the property
                #B This orders the authors' names by the Order and then extracts their names
                #C We simply count the number of reviews
                #D You can't have an average of nothing, so we check for that and return null. Otherwise we return the average of the star ratings
                * *******************************************************/
            }
        }

        //Fails - see https://github.com/aspnet/EntityFramework/issues/7714
        [Fact]
        public void TestIndividualBookListDtoOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var titles     = context.Books.Select(p => p.Title);
                var orgPrices  = context.Books.Select(p => p.Price);
                var actuaPrice = context.Books.Select(p => 
                                    p.Promotion == null
                                    ? p.Price : p.Promotion.NewPrice);
                var pText      = context.Books.Select(p => 
                                    p.Promotion == null
                                    ? null : p.Promotion.PromotionalText);
                var authorOrdered = 
                    context.Books.Select(p =>
                    string.Join(", ",
                        p.AuthorsLink
                            .OrderBy(q => q.Order)
                            .Select(q => q.Author.Name)));
                var reviewsCount = context.Books.Select(p => p.Reviews.Count);
                var reviewsAverageVotes = 
                    context.Books.Select(p => 
                       p.Reviews.Count == 0
                        ? null
                        : (decimal?) p.Reviews
                            .Select(q => q.NumStars).Average());

                //VERIFY
                titles.ToList();
                orgPrices.ToList();
                actuaPrice.ToList();
                pText.ToList();
                authorOrdered.ToList();
                reviewsCount.ToList();
                reviewsAverageVotes.ToList();
            }
        }

        //Fails - see https://github.com/aspnet/EntityFramework/issues/7714
        [Fact]
        public void TestAverageOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                var logIt = new LogDbContext(context);

                //ATTEMPT
                var dtos = context.Books.Select(p => new 
                {
                    ReviewsAverageVotes =                
                        p.Reviews.Count == 0             
                        ? null                           
                        : (decimal?)p                    
                              .Reviews                   
                              .Average(q => q.NumStars)  
                }).ToList();


                //VERIFY
                dtos.Any(x => x.ReviewsAverageVotes != null).ShouldBeTrue();
                foreach (var log in logIt.Logs)
                {
                    _output.WriteLine(log);
                }
                //to get the logs you need to fail see https://github.com/aspnet/Tooling/issues/541
                Assert.True(false, "failed the test so that the logs show");
            }
        }

        [Fact]
        public void TestDirectSelectBookListDtoOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                var logIt = new LogDbContext(context);

                //ATTEMPT
                var dtos = context.Books.Select(p => new BookListDto
                {
                    BookId = p.BookId,
                    Title = p.Title,
                    Price = p.Price,
                    PublishedOn = p.PublishedOn,
                    //ActualPrice = p.Promotion == null
                    //    ? p.Price
                    //    : p.Promotion.NewPrice,
                    //PromotionPromotionalText =
                    //    p.Promotion == null
                    //        ? null
                    //        : p.Promotion.PromotionalText,
                    //AuthorsOrdered = string.Join(", ",
                    //    p.AuthorsLink
                    //        .OrderBy(q => q.Order)
                    //        .Select(q => q.Author.Name)),
                    //ReviewsCount = p.Reviews.Count,
                    //ReviewsAverageVotes =
                    //    p.Reviews.Count == 0
                    //        ? null
                    //        : (decimal?)p.Reviews
                    //            .Select(q => q.NumStars).Average()
                }).ToList();

                //VERIFY
                dtos.Last().BookId.ShouldNotEqual(0);
                dtos.Last().Title.ShouldNotBeNull();
                dtos.Last().Price.ShouldNotEqual(0);
                //dtos.Last().ActualPrice.ShouldNotEqual(dtos.Last().Price);
                //dtos.Last().AuthorsOrdered.Length.ShouldBeInRange(1, 100);
                //dtos.Last().ReviewsCount.ShouldEqual(2);
                //dtos.Last().ReviewsAverageVotes.ShouldEqual(5);

                foreach (var log in logIt.Logs)
                {
                    _output.WriteLine(log);
                }
            }
        }

        [Fact]
        public void TestIQueryableSelectBookListDtoOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                var logIt = new LogDbContext(context);

                //ATTEMPT
                var dtos = context.Books.MapBookToDto().OrderByDescending(x => x.BookId).ToList();

                //VERIFY
                dtos.Last().BookId.ShouldNotEqual(0);
                dtos.Last().Title.ShouldNotBeNull();
                dtos.Last().Price.ShouldNotEqual(0);
                //dtos.Last().ActualPrice.ShouldNotEqual(dtos.Last().Price);
                dtos.Last().AuthorsOrdered.Length.ShouldBeInRange(1, 100);
                //dtos.Last().ReviewsCount.ShouldEqual(2);
                //dtos.Last().ReviewsAverageVotes.ShouldEqual(5);

                foreach (var log in logIt.Logs)
                {
                    _output.WriteLine(log);
                }
            }
        }

        [Fact]
        public void TestRawSqlOk()
        {
            //SETUP
            var options =
                this.ClassUniqueDatabaseSeeded4Books();

            using (var context = new EfCoreContext(options))
            {
                var logIt = new LogDbContext(context);

                //ATTEMPT
                var books =
                    context.Books.FromSql(
                            "SELECT * FROM dbo.books AS a ORDER BY (SELECT AVG(b.NumStars) FROM dbo.Review AS b WHERE b.BookId = a.BookId) DESC")
                        .ToList();

                //VERIFY
                books.First().Title.ShouldEqual("Quantum Networking");
                foreach (var log in logIt.Logs)
                {
                    _output.WriteLine(log);
                }
            }
        }
    }
}