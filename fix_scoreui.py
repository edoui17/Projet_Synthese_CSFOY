import sys

def replace_in_file(filepath, search_str, replace_str):
    with open(filepath, 'r') as file:
        content = file.read()
    if search_str in content:
        content = content.replace(search_str, replace_str)
        with open(filepath, 'w') as file:
            file.write(content)
        print(f"Replaced successfully in {filepath}")
    else:
        print(f"Search string not found in {filepath}")

filepath = "./Src/IslandSurvivor/Nodes/ScoreManager/ScoreUI.cs"

search = """    public override void _Ready()
    {
        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged += OnScoreChanged;
        }
        else
        {
            GD.PushWarning("ScoreUI: ScoreManager reference is missing.");
        }
    }"""

replace = """    public override void _Ready()
    {
        if (m_scoreManager == null)
        {
            // Try to find it dynamically globally
            m_scoreManager = GetTree().Root.GetNodeOrNull<ScoreManager>("Main/ScoreManager");
        }

        if (m_scoreManager == null)
        {
            // Alternative search using groups if it's there
            var currentScene = GetTree().CurrentScene;
            if (currentScene != null)
            {
                m_scoreManager = currentScene.GetNodeOrNull<ScoreManager>("ScoreManager");
            }
        }

        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged += OnScoreChanged;

            // Set initial score if possible
            Text = $"Score {m_scoreManager.GetCurrentScore()}";
        }
        else
        {
            GD.PushWarning("ScoreUI: ScoreManager reference is missing and could not be resolved.");
        }
    }"""

replace_in_file(filepath, search, replace)
