# Unity MCP setup status

The new project's `Packages/manifest.json` includes the official MCP for Unity Git URL (`https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`). This uses the current main-branch install path from the project's official installation guide and is scoped only to this project. Unity has not resolved/imported the package yet because the Editor stopped with license error 198 and the Unity Licensing Client is missing.

Detected client: VS Code with the Cline extension installed. The official package setup wizard can configure Cline automatically after the package imports. The setup guide requires Python 3.10+ and `uv`; neither is currently available on PATH (`py -0p` reports no installed Python runtimes, and `uv` was not found).

After signing into/activating Unity Hub and opening this project in Unity 6.6.6f1, allow Package Manager to resolve the Git dependency, complete the Window → MCP for Unity setup wizard, install its Python/`uv` prerequisites if prompted, select Cline in Configure Selected, and verify the status panel says Connected. No client-wide MCP settings were edited here.
