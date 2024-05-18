using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stin_backend.Controllers;
using stin_backend.Models;
using Xunit;

namespace Tests.Controller
{
    public class UserControllerTest
    {
        private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

        public UserControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "UsersControllerTests")
                .Options;
        }

        [Fact]
        public async Task GetUser_ShouldReturnAllUsers()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                if (!context.User.Any(u => u.id == 1))
                {
                    context.User.Add(new User { id = 1, username = "John", password = "password", pay = "100" });
                }

                if (!context.User.Any(u => u.id == 2))
                {
                    context.User.Add(new User { id = 2, username = "Jane", password = "password", pay = "200" });
                }

                context.SaveChanges();

                var controller = new UsersController(context);

                // Act
                var result = await controller.GetUser();

                // Assert
                result.Value.Should().BeAssignableTo<IEnumerable<User>>();

                var users = result.Value as IEnumerable<User>;
                users.Should().HaveCount(2);
                users.Should().ContainEquivalentOf(
                    new User { id = 1, username = "John", password = "password", pay = "100" },
                    options => options.ExcludingMissingMembers()
                );
                users.Should().ContainEquivalentOf(
                    new User { id = 2, username = "Jane", password = "password", pay = "200" },
                    options => options.ExcludingMissingMembers()
                );
            }
        }


        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new DatabaseContext(dbContextOptions))
            {
                context.User.Add(new User { id = 1, username = "John", password = "password", pay = "100" });
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(dbContextOptions))
            {
                var controller = new UsersController(context);

                // Act
                var actionResult = await controller.GetUser(1);

                // Assert
                var userResult = actionResult.Value as User;
                userResult.Should().NotBeNull();
                userResult.id.Should().Be(1);
            }
        }





        [Fact]
        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);

                var result = await controller.GetUser(999);

                result.Result.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task PutUser_ShouldReturnNoContent()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                context.User.Add(new User { id = 1, username = "John", password = "password", pay = "100" });
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);
                var userToUpdate = new User { id = 1, username = "John Updated", password = "newpassword", pay = "200" };

                var result = await controller.PutUser(1, userToUpdate);

                result.Should().BeOfType<NoContentResult>();

                var updatedUser = context.User.Find(1);
                updatedUser.username.Should().Be("John Updated");
                updatedUser.password.Should().Be("newpassword");
                updatedUser.pay.Should().Be("200");
            }
        }

        [Fact]
        public async Task PutUser_WithMismatchedId_ShouldReturnBadRequest()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);
                var userToUpdate = new User { id = 1, username = "John Updated", password = "newpassword", pay = "200" };

                var result = await controller.PutUser(2, userToUpdate);

                result.Should().BeOfType<BadRequestResult>();
            }
        }

        [Fact]
        public async Task PostUser_ShouldCreateUser()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                // Odstraníme existující uživatele
                context.User.RemoveRange(context.User);
                context.SaveChanges();
            }

            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);
                var newUser = new User { id = 1, username = "John", password = "password", pay = "100" };

                var result = await controller.PostUser(newUser);

                result.Result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().BeOfType<User>()
                    .Which.id.Should().Be(1);

                context.User.Should().ContainSingle(u => u.id == 1 && u.username == "John" && u.password == "password" && u.pay == "100");
            }
        }



        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                if (!context.User.Any(u => u.id == 1))
                {
                    context.User.Add(new User { id = 1, username = "John", password = "password", pay = "100" });
                    context.SaveChanges();
                }
            }

            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);

                // Ověříme, zda uživatel s ID 1 existuje před smazáním
                var userToDelete = await context.User.FindAsync(1);
                userToDelete.Should().NotBeNull(); // Ověříme, že uživatel s ID 1 existuje

                // Smazání uživatele
                var result = await controller.DeleteUser(1);

                result.Should().BeOfType<NoContentResult>(); // Ověříme, že byl vrácen NoContentResult

                // Ověříme, že kolekce uživatelů je po smazání prázdná
                context.User.Should().BeEmpty();
            }
        }



        [Fact]
        public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
        {
            using (var context = new DatabaseContext(_dbContextOptions))
            {
                var controller = new UsersController(context);

                // Pokud existuje uživatel s ID 999, smažeme ho, aby se ujistil, že smazání bude úspěšné
                var userToDelete = await context.User.FindAsync(999);
                if (userToDelete != null)
                {
                    context.User.Remove(userToDelete);
                    await context.SaveChangesAsync();
                }

                // Očekáváme, že požadavek na smazání uživatele s neplatným ID vrátí NotFoundResult
                var result = await controller.DeleteUser(999);

                result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}

