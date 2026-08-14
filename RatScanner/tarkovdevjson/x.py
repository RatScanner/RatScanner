import json
from pathlib import Path
import requests

# Base URL for the Tarkov Data API (update this to your target host)
BASE_URL = "https://json.tarkov.dev"

API_DOC = {
    "data": {
        "endpoints": [
            {
                "name": "barters",
                "path": "/{{gameMode}}/barters",
                "description": "trader barter offers",
                "translations": False,
            },
            {
                "name": "crafts",
                "path": "/{{gameMode}}/crafts",
                "description": "hideout crafts",
                "translations": False,
            },
            {
                "name": "hideout",
                "path": "/{{gameMode}}/hideout",
                "description": "hideout stations",
                "translations": True,
            },
            {
                "name": "items",
                "path": "/{{gameMode}}/items",
                "description": "items, categories, flea market, armor materials, player levels, mastering, skills",
                "translations": True,
            },
            {
                "name": "maps",
                "path": "/{{gameMode}}/maps",
                "description": "maps, goon reports, mobs (bosses), loot containers, stationary weapons",
                "translations": True,
            },
            {
                "name": "status",
                "path": "/status",
                "description": "EFT server status",
                "translations": False,
            },
            {
                "name": "tasks",
                "path": "/{{gameMode}}/tasks",
                "description": "tasks, quest items, achievements, prestige",
                "translations": True,
            },
            {
                "name": "traders",
                "path": "/{{gameMode}}/traders",
                "description": "traders",
                "translations": True,
            },
        ],
        "gameModes": ["regular", "pve", "pvp-season"],
        "languages": [
            "cs",
            "de",
            "en",
            "es",
            "fr",
            "hu",
            "id",
            "it",
            "ja",
            "ko",
            "pl",
            "pt",
            "ro",
            "ru",
            "sk",
            "th",
            "tr",
            "vn",
            "zh",
        ],
    }
}


def download_file(url: str, save_path: Path):
    """Helper function to fetch JSON and write to disk."""
    print(f"  Downloading -> {url}")
    try:
        response = requests.get(url, timeout=15)
        response.raise_for_status()

        with open(save_path, "w", encoding="utf-8") as f:
            json.dump(response.json(), f, indent=2, ensure_ascii=False)

        print(f"    ✓ Saved to {save_path}")
    except requests.exceptions.RequestException as e:
        print(f"    ✗ Failed: {e}")


def download_api_data(base_url: str, output_dir: str = "downloads"):
    data = API_DOC["data"]
    game_modes = data["gameModes"]
    endpoints = data["endpoints"]

    for mode in game_modes:
        mode_dir = Path(output_dir) / mode
        mode_dir.mkdir(parents=True, exist_ok=True)

        for endpoint in endpoints:
            path_template = endpoint["path"]

            # Build standard base relative path
            relative_path = path_template.replace("{{gameMode}}", mode)
            base_filename = endpoint["name"].replace(" ", "_")

            print(f"\nProcessing [{mode}] -> {endpoint['name']}")

            # 1. Download Base File
            base_url_full = f"{base_url.rstrip('/')}{relative_path}"
            base_save_path = mode_dir / f"{base_filename}.json"
            download_file(base_url_full, base_save_path)

            # 2. Download English Translation File (if endpoint supports translations)
            if endpoint.get("translations"):
                translation_url_full = f"{base_url.rstrip('/')}{relative_path}_en"
                translation_save_path = mode_dir / f"{base_filename}_en.json"
                download_file(translation_url_full, translation_save_path)


if __name__ == "__main__":
    download_api_data(BASE_URL)
