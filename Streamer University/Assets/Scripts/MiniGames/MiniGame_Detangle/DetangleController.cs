using System.Collections.Generic;
using UnityEngine;

public class DetangleController : MiniGameController
{
    [Header("Intro UI")]
    public GameObject introPanel;
    public GameObject gameUIRoot;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Game Settings")]
    public float timeLimit = 60f; // 60 seconds to solve
    private float timeRemaining;

    [Header("UI")]
    public TMPro.TMP_Text timerText;

    [Header("Game Setup")]
    public GameObject nodePrefab;  // Drag Node prefab
    public GameObject linePrefab; // Drag Line prefab
    public int numberOfNodes = 6;
    public float spawnRadius = 200f; // How spread out the nodes are

    [Header("Line Colors")]
    public Color lineNormalColor = Color.white;
    public Color lineIntersectColor = Color.red;
    public float lineWidth = 3f;

    [Header("Result Deltas")]
    public int fameDeltaOnWin = 10;
    public int stressDeltaOnWin = -5;
    public int fameDeltaOnLose = -5;
    public int stressDeltaOnLose = 10;

    private bool gameStarted = false;
    private bool hadIntersectionsAtStart = false;

    private List<GameObject> nodes = new List<GameObject>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<Vector2Int> connections = new List<Vector2Int>(); // Pairs of node indices that connect

    public static DetangleController Instance { get; private set; }
    public bool IsGameFinished => finished;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        this.delta = new Dictionary<string, int>();
        finished = false;
        successDeclared = false;

        if (string.IsNullOrEmpty(mySceneName))
            mySceneName = gameObject.scene.name;

        if (introPanel) introPanel.SetActive(true);
        if (gameUIRoot) gameUIRoot.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        gameStarted = false;

    }

    void CreateNodes()
    {
        for (int i = 0; i < numberOfNodes; i++)
        {
            GameObject node = Instantiate(nodePrefab, gameUIRoot.transform);

            // Position in a circle
            float angle = (i / (float)numberOfNodes) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * spawnRadius;
            float y = Mathf.Sin(angle) * spawnRadius;

            RectTransform rt = node.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);

            // Set node index
            Node draggable = node.GetComponent<Node>();
            if (draggable != null)
            {
                draggable.nodeIndex = i;
            }

            nodes.Add(node);
        }
    }

    void CreateConnections()
    {
        connections.Clear();
        lines.Clear();

        HashSet<string> existingConnections = new HashSet<string>();
        Dictionary<int, int> nodeConnectionCount = new Dictionary<int, int>();

        // Initialize connection count for all nodes
        for (int i = 0; i < numberOfNodes; i++)
        {
            nodeConnectionCount[i] = 0;
        }

        // First pass: Try to give every node at least 2 connections
        for (int i = 0; i < numberOfNodes; i++)
        {
            while (nodeConnectionCount[i] < 2)
            {
                // Find a node to connect to that also needs connections and doesn't have 3 yet
                int otherNode = Random.Range(0, numberOfNodes);

                // Skip if same node or other node already has 3 connections
                if (otherNode == i || nodeConnectionCount[otherNode] >= 3)
                {
                    //find any valid node
                    bool found = false;
                    for (int j = 0; j < numberOfNodes; j++)
                    {
                        if (j != i && nodeConnectionCount[j] < 3)
                        {
                            otherNode = j;
                            found = true;
                            break;
                        }
                    }
                    if (!found) break; // No valid nodes available
                }

                int min = Mathf.Min(i, otherNode);
                int max = Mathf.Max(i, otherNode);
                string key = min + "-" + max;

                // Only add if this connection doesn't already exist
                if (!existingConnections.Contains(key))
                {
                    existingConnections.Add(key);
                    connections.Add(new Vector2Int(i, otherNode));
                    nodeConnectionCount[i]++;
                    nodeConnectionCount[otherNode]++;
                }
            }
        }

        // Add a few more random connections where nodes have room (up to 3)
        int extraConnections = Random.Range(1, 4); // Add 1-3 extra connections
        int attempts = 0;
        int maxAttempts = 50;

        while (extraConnections > 0 && attempts < maxAttempts)
        {
            attempts++;
            int nodeA = Random.Range(0, numberOfNodes);
            int nodeB = Random.Range(0, numberOfNodes);

            if (nodeA == nodeB) continue;
            if (nodeConnectionCount[nodeA] >= 3) continue;
            if (nodeConnectionCount[nodeB] >= 3) continue;

            int min = Mathf.Min(nodeA, nodeB);
            int max = Mathf.Max(nodeA, nodeB);
            string key = min + "-" + max;

            if (!existingConnections.Contains(key))
            {
                existingConnections.Add(key);
                connections.Add(new Vector2Int(nodeA, nodeB));
                nodeConnectionCount[nodeA]++;
                nodeConnectionCount[nodeB]++;
                extraConnections--;
            }
        }

        // Debug: Log connection counts
        for (int i = 0; i < numberOfNodes; i++)
        {
            Debug.Log($"Node {i} has {nodeConnectionCount[i]} connections");
        }

        // Create UI line images
        foreach (var conn in connections)
        {
            GameObject lineObj = Instantiate(linePrefab, gameUIRoot.transform);
            lineObj.name = $"Line_{conn.x}_to_{conn.y}";

            UnityEngine.UI.Image lineImage = lineObj.GetComponent<UnityEngine.UI.Image>();
            if (lineImage != null)
            {
                lineImage.color = lineNormalColor;
                lineImage.raycastTarget = false;
            }

            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = lineObj.AddComponent<LineRenderer>();
            }

            lines.Add(lr);
        }

        Debug.Log($"Total connections created: {connections.Count}");
    }

    void UpdateLines()
    {
        // First, get all line positions
        List<Vector2> lineStarts = new List<Vector2>();
        List<Vector2> lineEnds = new List<Vector2>();

        for (int i = 0; i < connections.Count; i++)
        {
            var conn = connections[i];
            RectTransform nodeA = nodes[conn.x].GetComponent<RectTransform>();
            RectTransform nodeB = nodes[conn.y].GetComponent<RectTransform>();

            lineStarts.Add(nodeA.anchoredPosition);
            lineEnds.Add(nodeB.anchoredPosition);
        }

        // Track which lines are intersecting
        bool[] isLineIntersecting = new bool[connections.Count];
        bool anyIntersections = false;

        // Check all pairs of lines for intersections
        for (int i = 0; i < connections.Count; i++)
        {
            for (int j = i + 1; j < connections.Count; j++)
            {
                var conn1 = connections[i];
                var conn2 = connections[j];

                // Skip if lines share a node
                bool sharesNode = (conn1.x == conn2.x || conn1.x == conn2.y ||
                                  conn1.y == conn2.x || conn1.y == conn2.y);

                if (!sharesNode && DoLinesIntersect(lineStarts[i], lineEnds[i], lineStarts[j], lineEnds[j]))
                {
                    isLineIntersecting[i] = true;  // Mark BOTH lines as intersecting
                    isLineIntersecting[j] = true;
                    anyIntersections = true;
                }
            }
        }

        // Update line visuals
        for (int i = 0; i < connections.Count; i++)
        {
            if (i >= lines.Count) break;

            var conn = connections[i];
            GameObject lineObj = lines[i].gameObject;
            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            UnityEngine.UI.Image lineImage = lineObj.GetComponent<UnityEngine.UI.Image>();

            Vector2 posA = lineStarts[i];
            Vector2 posB = lineEnds[i];

            // Update line color based on intersection
            if (lineImage != null)
            {
                lineImage.color = isLineIntersecting[i] ? lineIntersectColor : lineNormalColor;
            }

            // Calculate direction and distance
            Vector2 direction = posB - posA;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Position line in the middle
            lineRect.anchoredPosition = (posA + posB) / 2f;

            // Set line width (distance) and rotate
            lineRect.sizeDelta = new Vector2(distance, lineWidth);
            lineRect.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Check win condition
        if (!anyIntersections && gameStarted && !finished && hadIntersectionsAtStart)
        {
            OnPlayerWin();
        }
    }

    void DrawUILine(LineRenderer lr, Vector2 start, Vector2 end)
    {
        // For now, let's just update the positions
        Vector3 worldStart = nodes[0].transform.parent.TransformPoint(start);
        Vector3 worldEnd = nodes[0].transform.parent.TransformPoint(end);

        lr.SetPosition(0, worldStart);
        lr.SetPosition(1, worldEnd);
    }

    public void OnNodeMoved(){}

    bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Line segment 1: p1 to p2
        // Line segment 2: p3 to p4

        float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (Mathf.Abs(d) < 0.0001f) return false; // Lines are parallel

        float t = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        // Check if intersection point is within both line segments
        return (t >= 0 && t <= 1 && u >= 0 && u <= 1);
    }
    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;

        if (introPanel) introPanel.SetActive(false);
        if (gameUIRoot) gameUIRoot.SetActive(true);

        CreateNodes();
        CreateConnections();

        // Move all nodes above lines in rendering order
        foreach (GameObject node in nodes)
        {
            node.transform.SetAsLastSibling();
        }

        timeRemaining = timeLimit;


        // Check if puzzle starts with intersections
        Invoke(nameof(CheckInitialIntersections), 0.1f); // Check after a tiny delay
    }

    void CheckInitialIntersections()
    {
        // Do one intersection check
        bool hasIntersections = CheckForAnyIntersections();

        if (hasIntersections)
        {
            hadIntersectionsAtStart = true;
        }
        else
        {
            // Regenerate the puzzle if no intersections
            Debug.Log("No intersections at start - regenerating puzzle");
            RegeneratePuzzle();
        }
    }

    bool CheckForAnyIntersections()
    {
        // Quick check to see if there are any intersections
        for (int i = 0; i < connections.Count; i++)
        {
            for (int j = i + 1; j < connections.Count; j++)
            {
                var conn1 = connections[i];
                var conn2 = connections[j];

                bool sharesNode = (conn1.x == conn2.x || conn1.x == conn2.y ||
                                  conn1.y == conn2.x || conn1.y == conn2.y);

                if (!sharesNode)
                {
                    RectTransform nodeA1 = nodes[conn1.x].GetComponent<RectTransform>();
                    RectTransform nodeB1 = nodes[conn1.y].GetComponent<RectTransform>();
                    RectTransform nodeA2 = nodes[conn2.x].GetComponent<RectTransform>();
                    RectTransform nodeB2 = nodes[conn2.y].GetComponent<RectTransform>();

                    if (DoLinesIntersect(nodeA1.anchoredPosition, nodeB1.anchoredPosition,
                                        nodeA2.anchoredPosition, nodeB2.anchoredPosition))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void RegeneratePuzzle()
    {
        // Destroy existing nodes and lines
        foreach (var node in nodes)
        {
            if (node != null) Destroy(node);
        }
        foreach (var line in lines)
        {
            if (line != null) Destroy(line.gameObject);
        }

        nodes.Clear();
        lines.Clear();
        connections.Clear();

        // Create new puzzle
        CreateNodes();
        CreateConnections();

        foreach (GameObject node in nodes)
        {
            node.transform.SetAsLastSibling();
        }

        // Check again
        Invoke(nameof(CheckInitialIntersections), 0.1f);
    }

    void Update()
    {
        if (!gameStarted || finished) return;

        UpdateLines();

        // Update timer
        timeRemaining -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.Ceil(timeRemaining)}s";
        }

        // Check lose condition
        if (timeRemaining <= 0f)
        {
            OnPlayerLose();
        }
    }

    public void OnPlayerWin()
    {
        if (finished) return;

        successDeclared = true;
        finished = true;
        gameStarted = false;

        if (winPanel) winPanel.SetActive(true);

        Invoke(nameof(FinishMiniGame), 3f);
    }

    public void OnPlayerLose()
    {
        if (finished) return;

        successDeclared = false;
        finished = true;
        gameStarted = false;

        if (losePanel) losePanel.SetActive(true);

        Invoke(nameof(FinishMiniGame), 3f);
    }

    public override void FinishMiniGame()
    {
        bool success = successDeclared;
        Debug.Log($"[Detangle] FinishMiniGame success={success}");

        this.delta = new Dictionary<string, int>();
        var setFlags = new List<string>();

        if (success)
        {
            this.delta["fame"] = fameDeltaOnWin;
            this.delta["stress"] = stressDeltaOnWin;
            setFlags.Add("detangleWin");
            EventManager.Instance.addToQueue("EVT_DETANGLE_WIN");
        }
        else
        {
            this.delta["fame"] = fameDeltaOnLose;
            this.delta["stress"] = stressDeltaOnLose;
            setFlags.Add("detangleLose");
            EventManager.Instance.addToQueue("EVT_DETANGLE_LOSE");
        }

        EventManager.Instance.setFlags(setFlags);

        var result = new MiniGameResult
        {
            success = success,
            delta = this.delta
        };

        if (resultChannel != null)
        {
            Debug.Log("[Detangle] Raising result through resultChannel.");
            resultChannel.Raise(result);
        }
    }
}