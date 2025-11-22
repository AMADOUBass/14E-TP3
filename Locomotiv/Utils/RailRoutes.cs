using System;
using System.Collections.Generic;
using System.Linq;
using Locomotiv.Model;

namespace Locomotiv.Utils
{
    public static class RailRoutes
    {
        private static readonly Dictionary<
            (string Depart, string Arrivee),
            List<string>
        > CheminsReels = new()
        {
            {
                ("Gare Québec-Gatineau", "Gare du palais"),
                new List<string> { "Gare Québec-Gatineau", "Gare du palais" }
            },
            {
                ("Gare du palais", "Gare Québec-Gatineau"),
                new List<string> { "Gare du palais", "Gare Québec-Gatineau" }
            },
            {
                ("Gare du palais", "Gare CN"),
                new List<string> { "Gare du palais", "Gare CN" }
            },
            {
                ("Gare CN", "Gare du palais"),
                new List<string> { "Gare CN", "Gare du palais" }
            },
            {
                ("Gare Québec-Gatineau", "Gare CN"),
                new List<string> { "Gare Québec-Gatineau", "Gare du palais", "Gare CN" }
            },
            {
                ("Gare CN", "Gare Québec-Gatineau"),
                new List<string> { "Gare CN", "Gare du palais", "Gare Québec-Gatineau" }
            },
            // Branche est
            {
                ("Gare du palais", "Port de Québec"),
                new List<string> { "Gare du palais", "Port de Québec" }
            },
            {
                ("Port de Québec", "Gare du palais"),
                new List<string> { "Port de Québec", "Gare du palais" }
            },
            {
                ("Port de Québec", "Baie de Beauport"),
                new List<string> { "Port de Québec", "Baie de Beauport" }
            },
            {
                ("Baie de Beauport", "Port de Québec"),
                new List<string> { "Baie de Beauport", "Port de Québec" }
            },
            {
                ("Baie de Beauport", "Vers Charlevoix"),
                new List<string> { "Baie de Beauport", "Vers Charlevoix" }
            },
            {
                ("Vers Charlevoix", "Baie de Beauport"),
                new List<string> { "Vers Charlevoix", "Baie de Beauport" }
            },
            // Composés
            {
                ("Gare du palais", "Vers Charlevoix"),
                new List<string>
                {
                    "Gare du palais",
                    "Port de Québec",
                    "Baie de Beauport",
                    "Vers Charlevoix",
                }
            },
            {
                ("Gare Québec-Gatineau", "Vers Charlevoix"),
                new List<string>
                {
                    "Gare Québec-Gatineau",
                    "Gare du palais",
                    "Port de Québec",
                    "Baie de Beauport",
                    "Vers Charlevoix",
                }
            },
            // Branche sud
            {
                ("Gare CN", "Vers la rive-sud"),
                new List<string> { "Gare CN", "Vers la rive-sud" }
            },
            {
                ("Vers la rive-sud", "Gare CN"),
                new List<string> { "Vers la rive-sud", "Gare CN" }
            },
            // Branche centre
            {
                ("Gare Québec-Gatineau", "Centre de distribution"),
                new List<string> { "Gare Québec-Gatineau", "Centre de distribution" }
            },
            {
                ("Centre de distribution", "Gare Québec-Gatineau"),
                new List<string> { "Centre de distribution", "Gare Québec-Gatineau" }
            },
            {
                ("Centre de distribution", "Vers la rive-sud"),
                new List<string> { "Centre de distribution", "Vers la rive-sud" }
            },
            {
                ("Vers la rive-sud", "Centre de distribution"),
                new List<string> { "Vers la rive-sud", "Centre de distribution" }
            },
            {
                ("Gare Québec-Gatineau", "Vers la rive-sud"),
                new List<string>
                {
                    "Gare Québec-Gatineau",
                    "Centre de distribution",
                    "Vers la rive-sud",
                }
            },
            // Branche ouest
            {
                ("Gare Québec-Gatineau", "Vers Gatineau"),
                new List<string> { "Gare Québec-Gatineau", "Vers Gatineau" }
            },
            {
                ("Vers Gatineau", "Gare Québec-Gatineau"),
                new List<string> { "Vers Gatineau", "Gare Québec-Gatineau" }
            },
            {
                ("Gare Québec-Gatineau", "Vers le nord"),
                new List<string> { "Gare Québec-Gatineau", "Vers le nord" }
            },
            {
                ("Vers le nord", "Gare Québec-Gatineau"),
                new List<string> { "Vers le nord", "Gare Québec-Gatineau" }
            },
        };

        /// <summary>
        /// Étend la liste d’arrêts choisie par l’utilisateur en insérant
        /// les étapes intermédiaires du réseau (sans doublons).
        /// </summary>
        public static List<PointArret> ExpandItinerary(
            List<PointArret> selectionUtilisateur,
            List<PointArret> tousLesPoints
        )
        {
            if (selectionUtilisateur == null || selectionUtilisateur.Count < 2)
                return selectionUtilisateur ?? new List<PointArret>();

            var result = new List<PointArret>();

            var parNom = tousLesPoints.GroupBy(p => p.Nom).ToDictionary(g => g.Key, g => g.First());

            for (int i = 0; i < selectionUtilisateur.Count - 1; i++)
            {
                var depart = selectionUtilisateur[i];
                var arrivee = selectionUtilisateur[i + 1];

                if (i == 0)
                    result.Add(depart);

                if (CheminsReels.TryGetValue((depart.Nom, arrivee.Nom), out var nomsChemin))
                {
                    for (int j = 1; j < nomsChemin.Count; j++)
                    {
                        var nom = nomsChemin[j];
                        if (parNom.TryGetValue(nom, out var pa))
                        {
                            if (!result.Any() || result.Last().Id != pa.Id)
                            {
                                result.Add(pa);
                            }
                        }
                    }
                }
                else
                {
                    if (!result.Any() || result.Last().Id != arrivee.Id)
                        result.Add(arrivee);
                }
            }

            return result;
        }
    }
}
