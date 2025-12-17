using System;
using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model;
using Locomotiv.Model.enums;
using Locomotiv.Utils;

namespace Locomotiv.Data
{
    public interface IDatabaseSeeder
    {
        void Seed(bool force = false);
        List<CommercialRoute> GetMockRoutes();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _db;

        public DatabaseSeeder(ApplicationDbContext db)
        {
            _db = db;
        }

        public void Seed(bool force = false)
        {
            if (!force && _db.Users.Any())
                return;

            Console.WriteLine("🔁 Exécution du seed...");

            if (force)
            {
                _db.Etapes.RemoveRange(_db.Etapes);
                _db.Itineraires.RemoveRange(_db.Itineraires);
                _db.Blocks.RemoveRange(_db.Blocks);
                _db.Trains.RemoveRange(_db.Trains);
                _db.PointArrets.RemoveRange(_db.PointArrets);
                _db.Stations.RemoveRange(_db.Stations);
                _db.Users.RemoveRange(_db.Users);
                _db.SaveChanges();
            }

            var (adminHash, adminSalt) = PassWordHelper.HashPassword("adminpass");
            var (employeHash, employeSalt) = PassWordHelper.HashPassword("employepass");
            var (clientComHash, clientComSalt) = PassWordHelper.HashPassword("clientCompass");
            var (clientHash, clientSalt) = PassWordHelper.HashPassword("clientpass");

            var gareQuebecGatineau = new Station
            {
                Nom = "Gare Québec-Gatineau", // 1
                Localisation = "Secteur ouest",
                Latitude = 46.794982825793305,
                Longitude = -71.33522613571915,
                CapaciteMaxTrains = 4,
                StationType = StationTypeEnum.Connexion.ToString(),
            };
            _db.Stations.Add(gareQuebecGatineau);

            var gareDuPalais = new Station
            {
                Nom = "Gare du palais", // 2
                Localisation = "Vieux-Québec",
                Latitude = 46.81755933580009,
                Longitude = -71.21384802568011,
                CapaciteMaxTrains = 5,
                StationType = StationTypeEnum.Port.ToString(),
            };
            _db.Stations.Add(gareDuPalais);

            var gareCn = new Station
            {
                Nom = "Gare CN", // 3
                Localisation = "Sainte-Foy",
                Latitude = 46.754337826264766,
                Longitude = -71.29767724672222,
                CapaciteMaxTrains = 1,
                StationType = StationTypeEnum.Logistique.ToString(),
            };
            _db.Stations.Add(gareCn);

            _db.SaveChanges();

            _db.Users.AddRange(
                new List<User>
                {
                    new()
                    {
                        Prenom = "Admin",
                        Nom = "Admin",
                        Username = "admin",
                        PasswordHash = adminHash,
                        PasswordSalt = adminSalt,
                        Role = UserRole.Admin,
                    },
                    new()
                    {
                        Prenom = "Employe",
                        Nom = "Employe",
                        Username = "employe",
                        PasswordHash = employeHash,
                        PasswordSalt = employeSalt,
                        Role = UserRole.Employe,
                        StationId = gareDuPalais.Id,
                        Station = gareDuPalais,
                    },
                    new()
                    {
                        Prenom = "Client",
                        Nom = "Client",
                        Username = "client",
                        PasswordHash = clientHash,
                        PasswordSalt = clientSalt,
                        Role = UserRole.Client,
                    },
                    new()
                    {
                        Prenom = "Client",
                        Nom = "Commercial",
                        Username = "clientcom",
                        PasswordHash = clientComHash,
                        PasswordSalt = clientComSalt,
                        Role = UserRole.ClientCommercial,
                    },
                }
            );
            _db.SaveChanges();

            var versCharlevoix = new PointArret
            {
                Nom = "Vers Charlevoix", // 4
                EstStation = false,
                Latitude = 46.89513912411967,
                Longitude = -71.12869379394515,
                Localisation = "Direction Charlevoix",
            };

            var baieDeBeauport = new PointArret
            {
                Nom = "Baie de Beauport", // 5
                EstStation = false,
                Latitude = 46.83840691613302,
                Longitude = -71.19593518550352,
                Localisation = "Baie de Beauport",
            };

            var portDeQuebec = new PointArret
            {
                Nom = "Port de Québec", // 6
                EstStation = false,
                Latitude = 46.821717937700434,
                Longitude = -71.20669657467614,
                Localisation = "Port de Québec",
            };

            var centreDistribution = new PointArret
            {
                Nom = "Centre de distribution", // 7
                EstStation = false,
                Latitude = 46.79312964904228,
                Longitude = -71.22721075321274,
                Localisation = "Zone industrielle / centre de distribution",
            };

            var versRiveSud = new PointArret
            {
                Nom = "Vers la rive-sud", // 8
                EstStation = false,
                Latitude = 46.712789536709145,
                Longitude = -71.27127908435082,
                Localisation = "Direction rive-sud / Lévis",
            };

            var versGatineau = new PointArret
            {
                Nom = "Vers Gatineau", // 9
                EstStation = false,
                Latitude = 46.77319899751728,
                Longitude = -71.48371966307195,
                Localisation = "Direction Gatineau",
            };

            var versNord = new PointArret
            {
                Nom = "Vers le nord", // 10
                EstStation = false,
                Latitude = 46.764968551442955,
                Longitude = -71.4717033680072,
                Localisation = "Direction nord",
            };

            var pGareQuebecGatineau = new PointArret
            {
                Nom = gareQuebecGatineau.Nom,
                EstStation = true,
                Latitude = gareQuebecGatineau.Latitude,
                Longitude = gareQuebecGatineau.Longitude,
                Localisation = gareQuebecGatineau.Localisation,
            };
            var pGareDuPalais = new PointArret
            {
                Nom = gareDuPalais.Nom,
                EstStation = true,
                Latitude = gareDuPalais.Latitude,
                Longitude = gareDuPalais.Longitude,
                Localisation = gareDuPalais.Localisation,
            };
            var pGareCn = new PointArret
            {
                Nom = gareCn.Nom,
                EstStation = true,
                Latitude = gareCn.Latitude,
                Longitude = gareCn.Longitude,
                Localisation = gareCn.Localisation,
            };

            _db.PointArrets.AddRange(
                new[]
                {
                    pGareQuebecGatineau,
                    pGareDuPalais,
                    pGareCn,
                    versCharlevoix,
                    baieDeBeauport,
                    portDeQuebec,
                    centreDistribution,
                    versRiveSud,
                    versGatineau,
                    versNord,
                }
            );
            _db.SaveChanges();

            var trainA = new Train
            {
                Nom = "Train A",
                Etat = EtatTrain.EnGare,
                Capacite = 2,
                StationId = gareDuPalais.Id,
                BlockId = null,
            };
            _db.Trains.Add(trainA);

            var trainB = new Train
            {
                Nom = "Train B",
                Etat = EtatTrain.EnGare,
                Capacite = 100,
                StationId = gareCn.Id,
                BlockId = null,
            };
            _db.Trains.Add(trainB);

            var trainTest = new Train
            {
                Nom = "Train Test",
                Etat = EtatTrain.EnGare,
                Capacite = 120,
                StationId = gareQuebecGatineau.Id,
                BlockId = null,
            };
            _db.Trains.Add(trainTest);

            _db.SaveChanges();

            var blocks = new List<Block>
            {
                new()
                {
                    Nom = "Gare Québec-Gatineau → Gare du palais",
                    LatitudeDepart = gareQuebecGatineau.Latitude,
                    LongitudeDepart = gareQuebecGatineau.Longitude,
                    LatitudeArrivee = gareDuPalais.Latitude,
                    LongitudeArrivee = gareDuPalais.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare du palais → Gare CN",
                    LatitudeDepart = gareDuPalais.Latitude,
                    LongitudeDepart = gareDuPalais.Longitude,
                    LatitudeArrivee = gareCn.Latitude,
                    LongitudeArrivee = gareCn.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare du palais → Port de Québec",
                    LatitudeDepart = gareDuPalais.Latitude,
                    LongitudeDepart = gareDuPalais.Longitude,
                    LatitudeArrivee = portDeQuebec.Latitude,
                    LongitudeArrivee = portDeQuebec.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Port de Québec → Baie de Beauport",
                    LatitudeDepart = portDeQuebec.Latitude,
                    LongitudeDepart = portDeQuebec.Longitude,
                    LatitudeArrivee = baieDeBeauport.Latitude,
                    LongitudeArrivee = baieDeBeauport.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Baie de Beauport → Vers Charlevoix",
                    LatitudeDepart = baieDeBeauport.Latitude,
                    LongitudeDepart = baieDeBeauport.Longitude,
                    LatitudeArrivee = versCharlevoix.Latitude,
                    LongitudeArrivee = versCharlevoix.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare CN → Vers la rive-sud",
                    LatitudeDepart = gareCn.Latitude,
                    LongitudeDepart = gareCn.Longitude,
                    LatitudeArrivee = versRiveSud.Latitude,
                    LongitudeArrivee = versRiveSud.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare Québec-Gatineau → Centre de distribution",
                    LatitudeDepart = gareQuebecGatineau.Latitude,
                    LongitudeDepart = gareQuebecGatineau.Longitude,
                    LatitudeArrivee = centreDistribution.Latitude,
                    LongitudeArrivee = centreDistribution.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Centre de distribution → Vers la rive-sud",
                    LatitudeDepart = centreDistribution.Latitude,
                    LongitudeDepart = centreDistribution.Longitude,
                    LatitudeArrivee = versRiveSud.Latitude,
                    LongitudeArrivee = versRiveSud.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare Québec-Gatineau → Vers Gatineau",
                    LatitudeDepart = gareQuebecGatineau.Latitude,
                    LongitudeDepart = gareQuebecGatineau.Longitude,
                    LatitudeArrivee = versGatineau.Latitude,
                    LongitudeArrivee = versGatineau.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
                new()
                {
                    Nom = "Gare Québec-Gatineau → Vers le nord",
                    LatitudeDepart = gareQuebecGatineau.Latitude,
                    LongitudeDepart = gareQuebecGatineau.Longitude,
                    LatitudeArrivee = versNord.Latitude,
                    LongitudeArrivee = versNord.Longitude,
                    Signal = SignalType.Vert,
                    EstOccupe = false,
                    TrainId = null,
                },
            };

            _db.Blocks.AddRange(blocks);
            _db.SaveChanges();

            Console.WriteLine("✅ Données initiales insérées (réseau + trains, sans itinéraires).");
        }

        public List<CommercialRoute> GetMockRoutes()
        {
            return new List<CommercialRoute>
            {
                new CommercialRoute
                {
                    TrainNumber = "T-101",
                    DepartureTime = DateTime.Now.AddHours(1),
                    ArrivalTime = DateTime.Now.AddHours(5),
                    TransitStations = "Montréal, Trois-Rivières",
                    AvailableWagons = 12,
                    CapacityTons = 250,
                    Status = "En cours",
                    EstimatedDelivery = "2 jours",
                    Price = 1500m,
                    Restrictions = "Pas de matières dangereuses.",
                    MarchandisesType = "Conteneurs",
                },
                new CommercialRoute
                {
                    TrainNumber = "T-202",
                    DepartureTime = DateTime.Now.AddHours(2),
                    ArrivalTime = DateTime.Now.AddHours(7),
                    TransitStations = "Québec, Lévis",
                    AvailableWagons = 8,
                    CapacityTons = 180,
                    Status = "Planifié",
                    EstimatedDelivery = "3 jours",
                    Price = 1200m,
                    Restrictions = "Température contrôlée requise.",
                    MarchandisesType = "Véhicules",
                },
                new CommercialRoute
                {
                    TrainNumber = "T-303",
                    DepartureTime = DateTime.Now.AddHours(-1),
                    ArrivalTime = DateTime.Now.AddHours(3),
                    TransitStations = "Ottawa",
                    AvailableWagons = 0,
                    CapacityTons = 0,
                    Status = "Terminé",
                    EstimatedDelivery = "1 jour",
                    Price = 2000m,
                    Restrictions = "Aucune.",
                    MarchandisesType = "Produits chimiques",
                },
                new CommercialRoute
                {
                    TrainNumber = "T-404",
                    DepartureTime = DateTime.Now.AddHours(6),
                    ArrivalTime = DateTime.Now.AddHours(12),
                    TransitStations = "Saguenay, Rimouski",
                    AvailableWagons = 5,
                    CapacityTons = 120,
                    Status = "Planifié",
                    EstimatedDelivery = "4 jours",
                    Price = 900m,
                    Restrictions = "Fragile.",
                    MarchandisesType = "Produits agricoles",
                },
            };
        }
    }
}