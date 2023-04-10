using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Users.Api.Logging;
using Users.Api.Models;
using Users.Api.Repositories;
using Users.Api.Services;
using Xunit;

namespace Users.Api.Tests.Unit.Application;

public class UserServiceTests
{
    private readonly UserService _sut;
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ILoggerAdapter<UserService> _logger = Substitute.For<ILoggerAdapter<UserService>>();

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _logger);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers_WhenSomeUsersExist()
    {
        // Arrange
        var nickChapsas = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nick Chapsas"
        };
        var expectedUsers = new[]
        {
            nickChapsas
        };
        _userRepository.GetAllAsync().Returns(expectedUsers);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        //result.Single().Should().BeEquivalentTo(nickChapsas);
        result.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetAllAsync_ShouldLogMessages_WhenInvoked()
    {
        // Arrange
        _userRepository.GetAllAsync().Returns(Enumerable.Empty<User>());

        // Act
        await _sut.GetAllAsync();

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Retrieving all users"));
        _logger.Received(1).LogInformation(Arg.Is("All users retrieved in {0}ms"), Arg.Any<long>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldLogMessageAndException_WhenExceptionIsThrown()
    {
        // Arrange
        var sqliteException = new SqliteException("Something went wrong", 500);
        _userRepository.GetAllAsync()
            .Throws(sqliteException);

        // Act
        var requestAction = async () => await _sut.GetAllAsync();

        // Assert
        await requestAction.Should()
            .ThrowAsync<SqliteException>().WithMessage("Something went wrong");
        _logger.Received(1).LogError(Arg.Is(sqliteException), Arg.Is("Something went wrong while retrieving all users"));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Enedy Cordeiro"
        };

        _userRepository.GetByIdAsync(user.Id).Returns(user);

        // Act
        var result = await _sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAUserDoesntExists()
    {
        // Arrange
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldLogCorrectMessages_WhenRetrievingTheUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId).ReturnsNull();

        // Act
        await _sut.GetByIdAsync(userId);

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Retrieving user with id: {0}"));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} retrieved in {1}ms"), Arg.Is(userId), Arg.Any<long>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldLogCorrectMessages_WhenAExceptionIsThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sqliteException = new SqliteException("Something went wrong", 500);
        _userRepository.GetByIdAsync(Arg.Any<Guid>())
                       .Throws(sqliteException);

        // Act
        var requestAction = async () => await _sut.GetByIdAsync(userId);

        // Assert
        await requestAction
                        .Should()
                        .ThrowAsync<SqliteException>()
                        .WithMessage("Something went wrong");

        _logger.Received(1).LogError(Arg.Is(sqliteException), Arg.Is("Something went wrong while retrieving user with id {0}"), Arg.Is(userId));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} retrieved in {1}ms"), Arg.Is(userId), Arg.Any<long>());
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAUser_WhenDetailsAreValid()
    {
        //  Arrange
        var user = new User
        {
            FullName = "Enedy Cordeiro"
        };

        _userRepository.CreateAsync(user).Returns(true);

        // Act
        var result = await _sut.CreateAsync(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldLogTheCorrectMessages_WhenCreationAUser()
    {
        //  Arrange
        var user = new User
        {
            FullName = "Enedy Cordeiro"
        };

        _userRepository.CreateAsync(user).Returns(true);

        // Act
        var result = await _sut.CreateAsync(user);

        result.Should().BeTrue();

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Creating user with id {0} and name: {1}"), Arg.Is(user.Id), Arg.Is(user.FullName));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} created in {1}ms"), Arg.Is(user.Id), Arg.Any<long>());
    }

    [Fact]
    public async Task CreateAsync_ShouldLogTheCorrectMessages_WhenAExceptionIsThrown()
    {
        // Arrange
        var sqliteException = new SqliteException("Something went wrong", 500);
        var user = new User
        {
            FullName = "Enedy Cordeiro"
        };
        _userRepository.CreateAsync(user).Throws(sqliteException);

        // Act
        var resultAction = async () => await _sut.CreateAsync(user);

        // Assert
        await resultAction
                .Should()
                .ThrowAsync<SqliteException>();

        _logger.Received(1).LogInformation(Arg.Is("Creating user with id {0} and name: {1}"), Arg.Is(user.Id), Arg.Is(user.FullName));
        _logger.Received(1).LogError(Arg.Is(sqliteException), Arg.Is("Something went wrong while creating a user"));
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldDeleteAUser_WhenUserExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.DeleteByIdAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteByIdAsync(id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldNotDeleteAUser_WhenUserDoesntExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.DeleteByIdAsync(id).Returns(false);

        // Act
        var result = await _sut.DeleteByIdAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldLogCorrectMessages_WhenInvoked()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.DeleteByIdAsync(id).Returns(true);

        // Act
        await _sut.DeleteByIdAsync(id);

        // Assert
        _logger.Received(1).LogInformation(Arg.Is("Deleting user with id: {0}"), Arg.Is(id));
        _logger.Received(1).LogInformation(Arg.Is("User with id {0} deleted in {1}ms"), Arg.Is(id), Arg.Any<long>());
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldLogCorrectMessages_WhenExceptionIsThrown()
    {
        // Arrange
        var sqliteException = new SqliteException("Something went wrong", 500);
        var id = Guid.NewGuid();
        _userRepository
                    .DeleteByIdAsync(id)
                    .Throws(sqliteException);

        // Act
        var action = async () => await _sut.DeleteByIdAsync(id);

        // Assert
        await action
                    .Should()
                    .ThrowAsync<SqliteException>();

        _logger.Received(1).LogInformation(Arg.Is("Deleting user with id: {0}"), Arg.Is(id));
        _logger.Received(1).LogError(sqliteException, Arg.Is("Something went wrong while deleting user with id {0}"), Arg.Is(id));
    }
}
