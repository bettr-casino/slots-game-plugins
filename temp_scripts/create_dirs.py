import os

# Base directories
base_dir_fbx = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/fbx"
base_dir_templates = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/templates/common"
base_dir_templates_integration = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/templates/integration"
base_dir_textures = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures"

# Function to create directories
def create_directories(base_dir, machine_name, machine_variant):
    machine_dir = os.path.join(base_dir, machine_name)
    variant_dir = os.path.join(machine_dir, machine_variant)
    if not os.path.exists(variant_dir):
        os.makedirs(variant_dir)
        print(f"Created directory: {variant_dir}")
    else:
        print(f"Directory already exists: {variant_dir}")

# List of file paths
file_paths = [
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersGalacticPioneers__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersCosmicRaiders__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersVoyagersOfTheCosmos__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersRaidersOfPlanetMooney__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersStellarExpedition__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersNebulaNavigators__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersInterstellarTreasureHunters__DefaultLobbyCard.jpg",
    "./Game009/LobbyCard/Materials/Game009__SpaceInvadersApolloAdventurers__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasMegaWheels__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasInfiniteSpins__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasTripleSpins__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasLucky7s__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasWildCherries__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasMegaJackpot__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasWheelBonanza__DefaultLobbyCard.jpg",
    "./Game007/LobbyCard/Materials/Game007__TrueVegasSuper7s__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__FantasyKingdom__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__GlobalInvestor__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__TreasureIslandTycoon__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__SpaceColonization__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__RealEstateMogul__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__EmpireBuilder__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__CapitalCityTycoon__DefaultLobbyCard.jpg",
    "./Game006/LobbyCard/Materials/Game006__IndustrialRevolution__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsSacredLegends__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsCelestialGuardians__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsHeavenlyMonarchs__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsTitansOfWealth__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsMysticPantheon__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsMythicDeities__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsDivineRiches__DefaultLobbyCard.jpg",
    "./Game008/LobbyCard/Materials/Game008__GodsEternalDivinity__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__JetsetJackpot__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__RubyRush__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__PharaohsRiches__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__RaceToRiches__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__SapphireSprint__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__DiamondDash__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__BillionaireBets__DefaultLobbyCard.jpg",
    "./Game004/LobbyCard/Materials/Game004__BeverlyHillsMansions__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesWonderWays__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesCascadingCash__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesAlpineAdventure__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesMegaMultipliers__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesHotLinks__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesStackedSpins__DefaultLobbyCard.jpg",
    "./Game003/LobbyCard/Materials/Game003__HighStakesMonacoThrills__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloSafariExpedition__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloAdventureQuest__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloFrontierFortune__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloWildPicks__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloTreasureHunter__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloWheelofRiches__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloWildSpins__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloJackpotMadness__DefaultLobbyCard.jpg",
    "./Game002/LobbyCard/Materials/Game002__BuffaloMagicSpins__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesManekiNeko__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesNorseAcorns__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesJadeOfFortune__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesMysticFortunes__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesKeysOfFortune__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesFourLeafClover__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesLuckyBamboo__DefaultLobbyCard.jpg",
    "./Game005/LobbyCard/Materials/Game005__FortunesShootingStars__DefaultLobbyCard.jpg"
]

for file_path in file_paths:
    # Extract filename
    file_name = os.path.basename(file_path)
    
    # Parse machine name and variant
    parts = file_name.split('__')
    if len(parts) == 3:
        machine_name = parts[0]
        machine_variant = parts[1]

        # Create directories for fbx, templates, and textures
        create_directories(base_dir_fbx, machine_name, machine_variant)
        create_directories(base_dir_templates, machine_name, machine_variant)
        create_directories(base_dir_templates_integration, machine_name, machine_variant)
        create_directories(base_dir_textures, machine_name, machine_variant)

print("Directory creation process completed.")

