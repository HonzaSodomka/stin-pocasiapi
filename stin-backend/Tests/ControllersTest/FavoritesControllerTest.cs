using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stin_backend.Controllers;
using stin_backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controller
{
    public class FavoritesControllerTest
    {
        [Fact]
        public async Task GetFavorite_ShouldReturnAllFavorites()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název databáze pro každý test
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                context.Favorite.AddRange(new List<Favorite>
                {
                    new Favorite { id = 1, city = "City1", User_id = 1 },
                    new Favorite { id = 2, city = "City2", User_id = 2 }
                });
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.GetFavorite();

                // Assert
                actionResult.Value.Should().NotBeNullOrEmpty();
                actionResult.Value.Should().BeAssignableTo<IEnumerable<Favorite>>();
                actionResult.Value.Should().HaveCount(2);
            }
        }


        [Fact]
        public async Task GetFavorite_WithValidId_ShouldReturnFavorite()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název databáze pro každý test
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                context.Favorite.Add(new Favorite { id = 100, city = "City1", User_id = 1 }); // Použití odlišného identifikátoru
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.GetFavorite(100); // Změna na použití nového identifikátoru

                // Assert
                actionResult.Value.Should().NotBeNull();
                actionResult.Value.Should().BeAssignableTo<Favorite>();
                var favorite = actionResult.Value as Favorite;
                favorite.id.Should().Be(100); // Změna na porovnání s novým identifikátorem
                favorite.city.Should().Be("City1");
            }
        }


        [Fact]
        public async Task PostFavorite_ShouldCreateNewFavorite()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název databáze pro každý test
                .Options;

            var newFavorite = new Favorite { id = 1, city = "NewCity", User_id = 1 };

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.PostFavorite(newFavorite);

                // Assert
                var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
                createdAtActionResult.Should().NotBeNull();
                createdAtActionResult.ActionName.Should().Be("GetFavorite");
                var createdFavorite = createdAtActionResult.Value as Favorite;
                createdFavorite.Should().NotBeNull();
                createdFavorite.id.Should().Be(newFavorite.id);
                createdFavorite.city.Should().Be(newFavorite.city);
            }
        }

        [Fact]
        public async Task PutFavorite_ShouldUpdateExistingFavorite()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název databáze pro každý test
                .Options;

            var updatedFavorite = new Favorite { id = 1, city = "UpdatedCity", User_id = 1 };

            using (var context = new DatabaseContext(dbContextOptions))
            {
                context.Favorite.Add(new Favorite { id = 1, city = "OriginalCity", User_id = 1 });
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.PutFavorite(1, updatedFavorite);

                // Assert
                actionResult.Should().BeOfType<NoContentResult>();
            }

            // Assert after the update
            using (var context = new DatabaseContext(dbContextOptions))
            {
                var updatedFavoriteFromDb = await context.Favorite.FindAsync(1);
                updatedFavoriteFromDb.Should().NotBeNull();
                updatedFavoriteFromDb.city.Should().Be(updatedFavorite.city);
            }
        }


        [Fact]
        public async Task DeleteFavorite_ShouldRemoveFavorite()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikátní název databáze pro každý test
                .Options;

            // Create and save the favorite
            using (var context = new DatabaseContext(dbContextOptions))
            {
                context.Favorite.Add(new Favorite { id = 1, city = "City1", User_id = 1 });
                context.SaveChanges();
            }

            // Act: Delete the favorite
            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.DeleteFavorite(1);
                // Assert
                actionResult.Should().BeOfType<NoContentResult>();
            }

            // Assert: Verify the favorite was deleted
            using (var context = new DatabaseContext(dbContextOptions))
            {
                var deletedFavorite = await context.Favorite.FindAsync(1);
                deletedFavorite.Should().BeNull();
            }
        }
        [Fact]
        public async Task GetFavorite_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.GetFavorite(100); // Invalid ID

                // Assert
                actionResult.Result.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task PutFavorite_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var updatedFavorite = new Favorite { id = 1, city = "UpdatedCity", User_id = 1 };

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.PutFavorite(1, updatedFavorite);

                // Assert
                actionResult.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task DeleteFavorite_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new FavoritesController(context);

                // Act
                var actionResult = await controller.DeleteFavorite(1);

                // Assert
                actionResult.Should().BeOfType<NotFoundResult>();
            }
        }

    }
}




