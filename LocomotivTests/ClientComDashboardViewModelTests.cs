using Locomotiv.Data;
using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace LocomotivTests
{
    public class ClientComDashboardViewModelTests
    {
        // Données de mock corrigées : initialise Price et MontantReservation (car PriceRestant est calculé)
        private readonly List<CommercialRoute> _mockRoutes = new()
        {
            // Route 1: PriceRestant = 1000m
            new CommercialRoute { MarchandisesType = "Conteneurs", DepartureTime = DateTime.Now.Date.AddHours(10), CapacityTons = 300, Price = 1000m, MontantReservation = 0m, AvailableWagons = 10 },
            // Route 2: PriceRestant = 2000m
            new CommercialRoute { MarchandisesType = "Véhicules", DepartureTime = DateTime.Now.Date.AddDays(1).AddHours(10), CapacityTons = 150, Price = 2000m, MontantReservation = 0m, AvailableWagons = 5 },
            // Route 3: PriceRestant = 1500m
            new CommercialRoute { MarchandisesType = "Conteneurs", DepartureTime = DateTime.Now.Date.AddHours(15), CapacityTons = 400, Price = 1500m, MontantReservation = 0m, AvailableWagons = 8 },
            // Route 4: PriceRestant = 800m
            new CommercialRoute { MarchandisesType = "Produits chimiques", DepartureTime = DateTime.Now.Date.AddDays(2).AddHours(15), CapacityTons = 100, Price = 800m, MontantReservation = 0m, AvailableWagons = 2 }
        };

        private Mock<IUserSessionService> CreerUserSessionMock()
        {
            var mock = new Mock<IUserSessionService>();
            mock.SetupAllProperties();
            mock.Object.ConnectedUser = new User { Username = "Test" };
            return mock;
        }

        // Simule l'interface IDatabaseSeeder
        private Mock<IDatabaseSeeder> CreerSeederMock()
        {
            var mock = new Mock<IDatabaseSeeder>();
            mock.Setup(s => s.GetMockRoutes()).Returns(_mockRoutes);
            return mock;
        }

        [Fact]
        public void ConnectedUser_DevraitRetournerUtilisateurConnecte()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            Assert.NotNull(vm.ConnectedUser);
            Assert.Equal("Test", vm.ConnectedUser.Username);
        }

        [Fact]
        public void FiltreParTypeMarchandise_FonctionneCorrectement()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.SelectedMarchandisesType = "Conteneurs";

            var routesFiltrees = vm.RoutesView.Cast<CommercialRoute>().ToList();

            Assert.Equal(2, routesFiltrees.Count);
            Assert.All(routesFiltrees, r => Assert.Equal("Conteneurs", r.MarchandisesType));
        }

        [Fact]
        public void FiltreParDate_FonctionneCorrectement()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            var date = DateTime.Now.Date;
            vm.SelectedDate = date;

            var routesFiltrees = vm.RoutesView.Cast<CommercialRoute>().ToList();

            Assert.Equal(2, routesFiltrees.Count);
            Assert.All(routesFiltrees, r => Assert.Equal(date, r.DepartureTime.Date));
        }

        [Fact]
        public void FiltreParCapacite_MinimumFonctionnel()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.MinCapacityTons = 250;

            var routesFiltrees = vm.RoutesView.Cast<CommercialRoute>().ToList();

            Assert.Equal(2, routesFiltrees.Count);
            Assert.All(routesFiltrees, r => Assert.True(r.CapacityTons >= 250));
        }

        [Fact]
        public void FiltreParWagons_MinimumFonctionnel()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.MinWagons = 8;

            var routesFiltrees = vm.RoutesView.Cast<CommercialRoute>().ToList();

            Assert.Equal(2, routesFiltrees.Count);
            Assert.All(routesFiltrees, r => Assert.True(r.AvailableWagons >= 8));
        }

        [Fact]
        public void FiltreParPrix_MaximumFonctionnel()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.MaxPrice = 1600m;

            var routesFiltrees = vm.RoutesView.Cast<CommercialRoute>().ToList();

            Assert.Equal(3, routesFiltrees.Count);
            Assert.All(routesFiltrees, r => Assert.True(r.PriceRestant <= 1600m));
        }

        [Fact]
        public void ResetFilters_RazTousLesFiltres()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.SelectedMarchandisesType = "Conteneurs";
            vm.SelectedDate = DateTime.Now;
            vm.MinCapacityTons = 100;
            vm.MinWagons = 2;
            vm.MaxPrice = 1500;

            vm.ResetFiltersCommand.Execute(null);

            Assert.Null(vm.SelectedMarchandisesType);
            Assert.Null(vm.SelectedDate);
            Assert.Null(vm.MinCapacityTons);
            Assert.Null(vm.MinWagons);
            Assert.Null(vm.MaxPrice);

            Assert.Equal(_mockRoutes.Count, vm.RoutesView.Cast<CommercialRoute>().Count());
        }

        [Fact]
        public void TriAscendantParDate_DepartureTime()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.SortCommand.Execute(null);

            var liste = vm.RoutesView.Cast<CommercialRoute>().ToList();
            var sorted = _mockRoutes.OrderBy(r => r.DepartureTime).ToList();

            Assert.Equal(sorted, liste);
        }

        [Fact]
        public void TriDescendantParDate_DepartureTime()
        {
            var mockSession = CreerUserSessionMock();
            var mockSeeder = CreerSeederMock();
            var vm = new ClientComDashboardViewModel(mockSession.Object, mockSeeder.Object);

            vm.SortDescendingCommand.Execute(null);

            var liste = vm.RoutesView.Cast<CommercialRoute>().ToList();
            var sorted = _mockRoutes.OrderByDescending(r => r.DepartureTime).ToList();

            Assert.Equal(sorted, liste);
        }
    }
}