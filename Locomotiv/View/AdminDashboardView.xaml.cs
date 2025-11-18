using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Locomotiv.Model;
using Locomotiv.Utils;
using Locomotiv.ViewModel;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Block;



namespace Locomotiv.View
{
    /// <summary>
    /// Logique d'interaction pour AdminDashboardViewModel.xaml
    /// </summary>
    public partial class AdminDashboardView : UserControl
    {
        private static readonly PointLatLng QuebecCenter = new PointLatLng(46.81750, -71.21376);
        public Station? SelectedStation { get; private set; }
        private readonly Dictionary<int, GMapMarker> _markersTrainsDynamiques = new Dictionary<int, GMapMarker>();
        // Id du block -> liste de points qui dessinent la vraie courbe du rail
        private readonly Dictionary<int, List<PointLatLng>> _blockPolylines = new();

        // ✅ Ajouts pour l’itinéraire et le train animé
        private GMapRoute? _itineraireRoute;
        private GMapMarker? _trainDynamiqueMarker;
        public AdminDashboardView()
        {
            InitializeComponent();

            //InitMap();

            Loaded += (s, e) =>
            {
                if (DataContext is AdminDashboardViewModel vm)
                {
                    InitMap();

                    InitBlockPolylines(vm.Blocks);
                    LoadJunctionMarkers();

                    LoadPointArretMarkers(vm.PointsInteret);
                    LoadStationMarkers(vm.GetStations());

                    LoadTrainMarkers(vm.GetTrainsEnMouvement());
                    LoadBlockRoutes(vm.Blocks);

                    vm.PropertyChanged += Vm_PropertyChanged;

                    var conflits = vm.GetConflits();
                    AfficherConflitsSurCarte(conflits);

                     
                }
            };

        }

        private void LoadJunctionMarkers()
        {
            // Liste de toutes les jonctions que tu veux afficher
            var jonctions = new List<PointLatLng>
    {
        RailGeometry.JonctionEstPalais,
        RailGeometry.JonctionCNSud,
        RailGeometry.JonctionCentre,
        RailGeometry.JonctionOuestGatineauNord
    };

            foreach (var j in jonctions)
            {
                var marker = new GMap.NET.WindowsPresentation.GMapMarker(j)
                {
                    Shape = new System.Windows.Shapes.Path
                    {
                        Data = System.Windows.Media.Geometry.Parse("M 0,5 L 5,0 L 10,5 L 5,10 Z"), // losange
                        Stroke = System.Windows.Media.Brushes.Black,
                        Fill = System.Windows.Media.Brushes.Gold,
                        StrokeThickness = 2,
                        ToolTip = "Jonction",
                    },
                    Offset = new System.Windows.Point(-5, -5)
                };

                MapControl.Markers.Add(marker);
            }
        }


        private void InitBlockPolylines(IEnumerable<Block> blocks)
        {
            _blockPolylines.Clear();

            foreach (var block in blocks)
            {
                // 1️⃣ D'abord essayer une géométrie réelle basée sur le nom du block
                if (RailGeometry.PolylinesParNomBlock.TryGetValue(block.Nom, out var realPoints)
                    && realPoints != null
                    && realPoints.Count >= 2)
                {
                    _blockPolylines[block.Id] = realPoints;
                    continue;
                }

                // 2️⃣ Sinon, fallback : courbe Bézier automatique entre départ & arrivée
                var start = new PointLatLng(block.LatitudeDepart, block.LongitudeDepart);
                var end = new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee);

                var points = new List<PointLatLng>();
                const int steps = 20;

                double midLat = (start.Lat + end.Lat) / 2;
                double midLng = (start.Lng + end.Lng) / 2;

                double curveStrength = 0.002;

                double dx = end.Lng - start.Lng;
                double dy = end.Lat - start.Lat;
                double length = Math.Sqrt(dx * dx + dy * dy);

                double px = length == 0 ? 0 : -(dy / length);
                double py = length == 0 ? 0 : (dx / length);

                var control = new PointLatLng(
                    midLat + py * curveStrength,
                    midLng + px * curveStrength
                );

                for (int i = 0; i <= steps; i++)
                {
                    double t = i / (double)steps;
                    double u = 1 - t;

                    double lat = u * u * start.Lat +
                                 2 * u * t * control.Lat +
                                 t * t * end.Lat;

                    double lng = u * u * start.Lng +
                                 2 * u * t * control.Lng +
                                 t * t * end.Lng;

                    points.Add(new PointLatLng(lat, lng));
                }

                _blockPolylines[block.Id] = points;
            }
        }


        private void RefreshTrainsDynamiques(IEnumerable<(int TrainId, PointLatLng Position)> positions)
        {
            // Supprimer les anciens markers dynamiques
            foreach (var marker in _markersTrainsDynamiques.Values)
            {
                MapControl.Markers.Remove(marker);
            }
            _markersTrainsDynamiques.Clear();

            // Recréer pour chaque train en mouvement add a pointille pour la route
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utils", "Assets", "rail.png");
            foreach (var (trainId, pos) in positions)
            {
                var marker = new GMapMarker(pos)
                {
                    Shape = new Image
                    {
                        Source = new BitmapImage(new Uri(path)),
                        Width = 30,
                        Height = 30,
                        ToolTip = $"Train {trainId} en mouvement"
                    },
                    Offset = new Point(-15, -15)
                };

                _markersTrainsDynamiques[trainId] = marker;
                MapControl.Markers.Add(marker);
            }
        }
        private void RefreshItineraireRoute(ObservableCollection<PointLatLng> points)
        {
            // Supprimer l'ancien itinéraire s’il existe
            if (_itineraireRoute != null)
            {
                MapControl.Markers.Remove(_itineraireRoute);
                _itineraireRoute = null;
            }

            if (points == null || points.Count < 2)
                return;

            _itineraireRoute = new GMapRoute(points)
            {
                Shape = new Path
                {
                    Stroke = Brushes.DeepPink,
                    StrokeThickness = 4,
                    Fill = Brushes.Transparent,
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    ToolTip = "Itinéraire du train"
                }
            };

            MapControl.Markers.Add(_itineraireRoute);
            RefreshMap();
        }
        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not AdminDashboardViewModel vm) return;

            if (e.PropertyName == nameof(AdminDashboardViewModel.ItineraireCourantPoints))
            {
                Dispatcher.Invoke(() => RefreshItineraireRoute(vm.ItineraireCourantPoints));
            }
            else if (e.PropertyName == nameof(AdminDashboardViewModel.SimulationsActives))
            {
                Dispatcher.Invoke(() => RefreshTrainsDynamiques(vm.SimulationsActives));
            }

        }
       
        public void RefreshMap()
        {
            MapControl.InvalidateVisual();
            MapControl.UpdateLayout();
        }




        private void InitMap()
        {

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            MapControl.MapProvider = GMapProviders.OpenStreetMap;

            MapControl.Position = QuebecCenter;

            MapControl.MinZoom = 5;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 13;


            MapControl.CanDragMap = true;
            MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            MapControl.IgnoreMarkerOnMouseWheel = true;
            MapControl.ShowCenter = false;


            MapControl.Markers.Clear();


        }
       
        public void LoadPointArretMarkers(IEnumerable<PointArret> pointsArret)
        {
            //MapControl.Markers.Clear(); // Réinitialise la carte

            foreach (var arret in pointsArret)
            {
                if (arret.Latitude == 0 && arret.Longitude == 0)
                    continue;

                var position = new PointLatLng(arret.Latitude, arret.Longitude);
                var couleur = arret.EstStation ? Brushes.Blue : Brushes.Green;
                var remplissage = arret.EstStation ? Brushes.LightBlue : Brushes.LightGreen;
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utils", "Assets", "Poi.jpg");
                var marker = new GMapMarker(position)
                {
                    Shape = new Image
                    {
                        Source = new BitmapImage(new Uri(path)),
                        Width = 30,
                        Height = 30,
                        ToolTip = arret.Nom
                    },
                    Offset = new Point(-15, -15),
                    Tag = arret
                };

                marker.Shape.MouseLeftButtonUp += (s, e) =>
                {
                    if (DataContext is AdminDashboardViewModel vm)
                    {
                        vm.PointArretSelectionne = marker.Tag as PointArret;
                        MapControl.Position = position;
                        MapControl.Zoom = 15;
                    }
                };

                MapControl.Markers.Add(marker);
            }
        }

        public void LoadStationMarkers(IEnumerable<Station> stations)
        {
            //MapControl.Markers.Clear();
            foreach (var station in stations)
            {
                if (station.Latitude == 0 && station.Longitude == 0)
                {
                    Console.WriteLine($"Coordonnées invalides pour : {station.Nom}");
                    continue;
                }

                var position = new PointLatLng(station.Latitude, station.Longitude);
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utils", "Assets", "station.png");
                var marker = new GMapMarker(position)
                {
                    Shape = new Image
                    {
                        Source = new BitmapImage(new Uri(path)),
                        Width = 30,
                        Height = 30,
                        ToolTip = station.Nom + $" (Capacité: {station.CapaciteMaxTrains} Train)"
                    },
                    Offset = new Point(-15, -15),
                    Tag = station
                };
                marker.Shape.MouseLeftButtonUp += (s, e) =>
                {
                    if (DataContext is AdminDashboardViewModel vm)
                    {
                        vm.StationSelectionnee = marker.Tag as Station;
                        MapControl.Position = position;
                        MapControl.Zoom = 15;
                    }
                };
                MapControl.Markers.Add(marker);

            }
        }

        private Brush GetTrainColor(EtatTrain etat) => etat switch
        {
            EtatTrain.EnGare => Brushes.Blue,
            EtatTrain.EnTransit => Brushes.Orange,
            EtatTrain.EnAttente => Brushes.Gray,
            EtatTrain.HorsService => Brushes.Black,
            EtatTrain.Programme => Brushes.Purple,
            _ => Brushes.LightGray
        };
        public void LoadTrainMarkers(IEnumerable<Train> Train)
        {
            foreach (var train in Train)
            {
                PointLatLng position;

                if (train.Etat == EtatTrain.EnTransit && train.Block != null)
                {
                    // Position au milieu du block
                    var lat = (train.Block.LatitudeDepart + train.Block.LatitudeArrivee) / 2;
                    var lng = (train.Block.LongitudeDepart + train.Block.LongitudeArrivee) / 2;
                    position = new PointLatLng(lat, lng);
                }
                else if (train.Station != null)
                {
                    position = new PointLatLng(train.Station.Latitude, train.Station.Longitude);
                }
                else continue;
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Utils", "Assets", "trainI.png");
                var marker = new GMapMarker(position)
                {
                    Shape = new Image
                    {
                        Source = new BitmapImage(new Uri(path)),
                        Width = 30,
                        Height = 30,
                        ToolTip = $"Train: {train.Nom}\nÉtat: {train.Etat}"
                    },
                    Offset = new Point(-15, -15),
                };

                MapControl.Markers.Add(marker);
            }
        }
        private Brush GetBlockColor(SignalType signal) => signal switch
        {
            SignalType.Vert => Brushes.Green,
            SignalType.Rouge => Brushes.Red,
            SignalType.Jaune => Brushes.Yellow,
            _ => Brushes.Gray
        };
        private double GetAngleBetweenPoints(PointLatLng start, PointLatLng end)
        {
            double deltaX = end.Lng - start.Lng;
            double deltaY = end.Lat - start.Lat;
            double angleRad = Math.Atan2(deltaY, deltaX);
            return angleRad * (180 / Math.PI); // convertit en degrés
        }
        public void LoadBlockRoutes(IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                // On récupère la polyline si définie, sinon on retombe sur un simple segment
                List<PointLatLng>? points;

                if (!_blockPolylines.TryGetValue(block.Id, out points) || points == null || points.Count < 2)
                {
                    points = new List<PointLatLng>
                    {
                        new PointLatLng(block.LatitudeDepart, block.LongitudeDepart),
                        new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee)
                    };
                }

                var route = new GMapRoute(points)
                {
                    Shape = new System.Windows.Shapes.Path
                    {
                        Stroke = GetBlockColor(block.Signal),
                        StrokeThickness = block.EstOccupe ? 6 : 4,
                        Opacity = block.EstOccupe ? 0.8 : 0.4,
                        ToolTip = $"🛤️ Block: {block.Nom}\n🚦 Signal: {block.Signal}\n📍 Occupé: {(block.EstOccupe ? "Oui" : "Non")}",
                        IsHitTestVisible = true
                    }
                };

                MapControl.Markers.Add(route);

                // Flèche directionnelle : on la met à la fin de la polyline
                var last = points.Last();
                var first = points.First();

                var directionMarker = new GMapMarker(last)
                {
                    Shape = new System.Windows.Shapes.Path
                    {
                        Data = System.Windows.Media.Geometry.Parse("M 0,0 L 10,5 L 0,10 Z"),
                        Fill = System.Windows.Media.Brushes.Black,
                        Width = 10,
                        Height = 10,
                        ToolTip = $"→ {block.Nom}",
                        RenderTransform = new System.Windows.Media.RotateTransform(
                            GetAngleBetweenPoints(first, last)),
                        RenderTransformOrigin = new System.Windows.Point(0.5, 0.5)
                    },
                    Offset = new System.Windows.Point(-5, -5)
                };

                MapControl.Markers.Add(directionMarker);
            }
        }

        public void AfficherConflitsSurCarte(List<(Train TrainA, Train TrainB, Block BlockConflit)> conflits)
        {
            if (conflits == null || conflits.Count == 0) return;

            foreach (var (trainA, trainB, block) in conflits)
            {
                var start = new PointLatLng(block.LatitudeDepart, block.LongitudeDepart);
                var end = new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee);

                var startLocal = MapControl.FromLatLngToLocal(start);
                var endLocal = MapControl.FromLatLngToLocal(end);

                var strokeBrush = new SolidColorBrush(Colors.Red);

                var line = new Line
                {
                    X1 = startLocal.X,
                    Y1 = startLocal.Y,
                    X2 = endLocal.X,
                    Y2 = endLocal.Y,
                    StrokeThickness = 8,
                    Stroke = strokeBrush,
                    Fill = Brushes.Transparent,
                    ToolTip = $"⚠️ Conflit entre {trainA.Nom} et {trainB.Nom}\nBlock: {block.Nom}",
                    IsHitTestVisible = true
                };

                // Tooltip stable
                ToolTipService.SetInitialShowDelay(line, 0);
                ToolTipService.SetShowDuration(line, 5000);
                ToolTipService.SetPlacement(line, PlacementMode.Mouse);

                // Glow effect
                line.Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };

                // Hover interaction
                line.MouseEnter += (s, e) =>
                {
                    line.StrokeThickness = 10;
                    line.Effect = new DropShadowEffect { Color = Colors.Yellow, BlurRadius = 15 };
                };
                line.MouseLeave += (s, e) =>
                {
                    line.StrokeThickness = 8;
                    line.Effect = new DropShadowEffect { Color = Colors.Red, BlurRadius = 10 };
                };

                var canvas = new Canvas();
                canvas.Children.Add(line);

                // Animation rouge ↔ orange
                canvas.Loaded += (s, e) =>
                {
                    var colorAnimation = new ColorAnimation
                    {
                        From = Colors.Red,
                        To = Colors.Orange,
                        Duration = TimeSpan.FromSeconds(0.5),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(colorAnimation);
                    Storyboard.SetTarget(colorAnimation, strokeBrush);
                    Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
                    storyboard.Begin();
                };

                // Marqueur principal
                var conflictMarker = new GMapMarker(start)
                {
                    Shape = canvas,
                    Offset = new Point(0, 0)
                };
                MapControl.Markers.Add(conflictMarker);

                // 🚨 Icône d’alerte à l’arrivée
                var alertMarker = new GMapMarker(end)
                {
                    Shape = new TextBlock
                    {
                        Text = "🚨",
                        FontSize = 24,
                        Foreground = Brushes.Red,
                        ToolTip = $"Conflit détecté sur {block.Nom}"
                    },
                    Offset = new Point(-12, -12)
                };
                MapControl.Markers.Add(alertMarker);

                // 🏷️ Label avec noms des Train
                var labelMarker = new GMapMarker(start)
                {
                    Shape = new TextBlock
                    {
                        Text = $"{trainA.Nom} ↔ {trainB.Nom}",
                        FontSize = 12,
                        Foreground = Brushes.White,
                        Background = Brushes.DarkRed,
                        Padding = new Thickness(2),
                        ToolTip = $"Conflit sur {block.Nom}"
                    },
                    Offset = new Point(-30, -30)
                };
                MapControl.Markers.Add(labelMarker);
            }

            // Zoom uniquement si un seul conflit
            if (conflits.Count == 1)
            {
                var first = conflits.First().BlockConflit;
                MapControl.Position = new PointLatLng(first.LatitudeDepart, first.LongitudeDepart);
                MapControl.Zoom = 16;
            }
        }
        private void AfficherConflits_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
            {
                var conflits = vm.GetConflits();
                AfficherConflitsSurCarte(conflits);
            }
        }
        private void RafraichirConflits_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
            {
                vm.ConflitsTextuels = new ObservableCollection<string>(
                    vm.GetConflits().Select(c => $"⚠️ {c.TrainA.Nom} et {c.TrainB.Nom} sur {c.BlockConflit.Nom}")
                );
            }
        }
        private void AdminDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
            {
                var conflits = vm.GetConflits();
                AfficherConflitsSurCarte(conflits); // ✅ méthode locale dans le .xaml.cs
            }
        }
      
    }
}


