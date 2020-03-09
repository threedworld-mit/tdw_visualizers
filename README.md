# README

The visualizer applications can display using TDW assets using images created by the "screenshotter" controller. Use the visualizers to see all of the assets in a library at a glance, to search for assets by name, and to review some of the metadata.

The Model Visualizer will show images of models. The Material Visualizer will show images of visual materials.

![](images/model_visualizer.png)

## Usage

1. [**Download the visualizer binaries.**](https://github.com/threedworld-mit/tdw_visualizers/releases/latest)

2. **Generate the images with the "screenshotter" controller.** This can be found in the tdw repo.
    - `cd <root>/tdw/Python`
    - _Models:_ `python3 screenshotter.py` _Materials:_ `python3 screenshotter.py --type materials`
    - Run build

3. **Run the visualizer application.**
