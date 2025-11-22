using GMap.NET;

namespace Locomotiv.Utils
{
    /// <summary>
    /// Géométrie des rails pour chaque block du réseau.
    /// </summary>
    public static class RailGeometry
    {
        // ======================
        // Points de base (stations & POI)
        // ======================

        public static readonly PointLatLng GareQuebecGatineau = new PointLatLng(
            46.794982825793305,
            -71.33522613571915
        );

        public static readonly PointLatLng GareDuPalais = new PointLatLng(
            46.81755933580009,
            -71.21384802568011
        );

        public static readonly PointLatLng GareCN = new PointLatLng(
            46.754337826264766,
            -71.29767724672222
        );

        public static readonly PointLatLng VersCharlevoix = new PointLatLng(
            46.89513912411967,
            -71.12869379394515
        );

        public static readonly PointLatLng BaieDeBeauport = new PointLatLng(
            46.83840691613302,
            -71.19593518550352
        );

        public static readonly PointLatLng PortDeQuebec = new PointLatLng(
            46.821717937700434,
            -71.20669657467614
        );

        public static readonly PointLatLng CentreDistribution = new PointLatLng(
            46.79312964904228,
            -71.22721075321274
        );

        public static readonly PointLatLng VersRiveSud = new PointLatLng(
            46.712789536709145,
            -71.27127908435082
        );
        public static readonly PointLatLng VersGatineau = new PointLatLng(
            46.77319899751728,
            -71.48371966307195
        );

        public static readonly PointLatLng VersNord = new PointLatLng(
            46.764968551442955,
            -71.4717033680072
        );

        // ======================
        // Jonctions (embranchements)
        // ======================

        /// <summary>
        /// Jonction proche de la Gare du Palais (bifurcation vers Port / Baie).
        /// </summary>
        public static readonly PointLatLng JonctionEstPalais = new PointLatLng(46.8190, -71.2100);

        /// <summary>
        /// Jonction après la Gare CN (bifurcation vers Rive-sud).
        /// </summary>
        public static readonly PointLatLng JonctionCNSud = new PointLatLng(46.7500, -71.2900);

        /// <summary>
        /// Jonction entre Québec-Gatineau, Centre de distribution et Rive-sud.
        /// </summary>
        public static readonly PointLatLng JonctionCentre = new PointLatLng(46.7850, -71.2650);

        /// <summary>
        /// Jonction ouest de Québec-Gatineau (vers Gatineau / Nord).
        /// </summary>
        public static readonly PointLatLng JonctionOuestGatineauNord = new PointLatLng(
            46.7800,
            -71.3900
        );

        // ======================
        // Polylines par nom de block
        // ======================

        public static readonly Dictionary<string, List<PointLatLng>> PolylinesParNomBlock =
            new Dictionary<string, List<PointLatLng>>
            {
                // ===============================================
                // 1) Gare Québec-Gatineau → Gare du palais
                // ===============================================
                {
                    "Gare Québec-Gatineau → Gare du palais",
                    new List<PointLatLng>
                    {
                        GareQuebecGatineau,
                        new PointLatLng(46.7955, -71.3200),
                        new PointLatLng(46.8000, -71.3000),
                        new PointLatLng(46.8050, -71.2800),
                        new PointLatLng(46.8100, -71.2550),
                        JonctionEstPalais,
                        GareDuPalais,
                    }
                },
                // ===============================================
                // 2) Gare du palais → Gare CN
                // ===============================================
                {
                    "Gare du palais → Gare CN",
                    new List<PointLatLng>
                    {
                        GareDuPalais,
                        new PointLatLng(46.8120, -71.2300),
                        new PointLatLng(46.8050, -71.2450),
                        new PointLatLng(46.7950, -71.2600),
                        new PointLatLng(46.7800, -71.2750),
                        GareCN,
                    }
                },
                // ===============================================
                // 3) Gare du palais → Port de Québec
                // ===============================================
                {
                    "Gare du palais → Port de Québec",
                    new List<PointLatLng>
                    {
                        GareDuPalais,
                        new PointLatLng(46.8180, -71.2125),
                        JonctionEstPalais,
                        new PointLatLng(46.8205, -71.2085),
                        PortDeQuebec,
                    }
                },
                // ===============================================
                // 4) Port de Québec → Baie de Beauport
                // ===============================================
                {
                    "Port de Québec → Baie de Beauport",
                    new List<PointLatLng>
                    {
                        PortDeQuebec,
                        new PointLatLng(46.8240, -71.2050),
                        new PointLatLng(46.8300, -71.2015),
                        new PointLatLng(46.8350, -71.1995),
                        BaieDeBeauport,
                    }
                },
                // ===============================================
                // 5) Baie de Beauport → Vers Charlevoix
                // ===============================================
                {
                    "Baie de Beauport → Vers Charlevoix",
                    new List<PointLatLng>
                    {
                        BaieDeBeauport,
                        new PointLatLng(46.8500, -71.1850),
                        new PointLatLng(46.8700, -71.1600),
                        new PointLatLng(46.8850, -71.1400),
                        VersCharlevoix,
                    }
                },
                // ===============================================
                // 6) Gare CN → Vers la rive-sud
                // ===============================================
                {
                    "Gare CN → Vers la rive-sud",
                    new List<PointLatLng>
                    {
                        GareCN,
                        new PointLatLng(46.7520, -71.2950),
                        JonctionCNSud,
                        new PointLatLng(46.7450, -71.2900),
                        VersRiveSud,
                    }
                },
                // ===============================================
                // 7) Gare Québec-Gatineau → Centre de distribution
                // ===============================================
                {
                    "Gare Québec-Gatineau → Centre de distribution",
                    new List<PointLatLng>
                    {
                        GareQuebecGatineau,
                        new PointLatLng(46.7945, -71.3200),
                        new PointLatLng(46.7940, -71.3000),
                        new PointLatLng(46.7935, -71.2850),
                        CentreDistribution,
                    }
                },
                // ===============================================
                // 8) Centre de distribution → Vers la rive-sud
                // ===============================================
                {
                    "Centre de distribution → Vers la rive-sud",
                    new List<PointLatLng>
                    {
                        CentreDistribution,
                        JonctionCentre,
                        new PointLatLng(46.7600, -71.2750),
                        VersRiveSud,
                    }
                },
                // ===============================================
                // 9) Gare Québec-Gatineau → Vers Gatineau
                // ===============================================
                {
                    "Gare Québec-Gatineau → Vers Gatineau",
                    new List<PointLatLng>
                    {
                        GareQuebecGatineau,
                        new PointLatLng(46.7900, -71.3600),
                        JonctionOuestGatineauNord,
                        new PointLatLng(46.7800, -71.4200),
                        new PointLatLng(46.7780, -71.4500),
                        VersGatineau,
                    }
                },
                // ===============================================
                // 10) Gare Québec-Gatineau → Vers le nord
                // ===============================================
                {
                    "Gare Québec-Gatineau → Vers le nord",
                    new List<PointLatLng>
                    {
                        GareQuebecGatineau,
                        new PointLatLng(46.7920, -71.3550),
                        JonctionOuestGatineauNord,
                        new PointLatLng(46.7750, -71.4300),
                        VersNord,
                    }
                },
            };
    }
}
