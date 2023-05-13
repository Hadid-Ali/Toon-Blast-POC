using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    public GamePools gamePools;

    public GameUi gameUi;

    public GameObject goalPrefab;
    public ObjectPool scoreTextPool;

    public float blockFallSpeed;

    public float horizontalSpacing;
    public float verticalSpacing;

    public Transform levelLocation;

    public Sprite backgroundSprite;
    public Color backgroundColor;

    public List<AudioClip> gameSounds;


    [SerializeField] private RectTransform canvas;
    public LevelData LevelData;

    [HideInInspector] public List<GameObject> tileEntities = new List<GameObject>();
    public readonly List<GameObject> blockers = new List<GameObject>();
    [HideInInspector] public List<Vector2> tilePositions = new List<Vector2>();

    public Image ingameBoosterPanel;
    public Text ingameBoosterText;

    private readonly GameState gameState = new GameState();

    private bool gameStarted;
    private bool gameFinished;

    private float accTime;

    private float blockWidth;
    private float blockHeight;

    private bool suggestedMatch;
    private readonly List<GameObject> suggestedMatchBlocks = new List<GameObject>();

    private int currentLimit;

    private bool currentlyAwardingBoosters;

    private Coroutine countdownCoroutine;

    private bool boosterMode;

    private const float timeBetweenMatchSuggestions = 5.0f;
    private const float endGamePopupDelay = 0.75f;

    private bool applyingPenalty;

    private int ingameBoosterBgTweenId;

    private int generatedCollectables;
    private int neededCollectables;

    private Camera mainCamera;

    private int m_BlockScore;

    /// <summary>
    /// Unity's Start method.
    /// </summary>
    private void Start()
    {
        m_BlockScore = GameData.StaticData.BlockScore;
        mainCamera = Camera.main;
        LevelData = LevelDataRegistory.Instance.GetLevelData(1);

        ResetLevelData();
        CreateBackgroundTiles();

        SoundManager.Instance.AddSounds(gameSounds);
    }

    private void OnDestroy()
    {
        SoundManager.Instance.RemoveSounds(gameSounds);
    }

    /// <summary>
    /// Unity's Update method.
    /// </summary>
    private void Update()
    {
        if (boosterMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Block"))
                {
                    var hitIdx = tileEntities.FindIndex(x => x == hit.collider.gameObject);
                    var hitBlock = hit.collider.gameObject.GetComponent<TileEntity>();
                    if (blockers[hitIdx] != null)
                    {
                        return;
                    }

                    if (IsBoosterBlock(hitBlock))
                    {
                        return;
                    }

                    hitBlock.GetComponent<PooledObject>().pool.ReturnObject(hitBlock.gameObject);
                    CreateBooster(BoosterType.Dynamite, hitIdx);
                }

                boosterMode = false;
                FadeOutInGameBoosterOverlay();
            }

            return;
        }

        accTime += Time.deltaTime;
        if (accTime >= timeBetweenMatchSuggestions)
        {
            accTime = 0.0f;
            HighlightRandomMatch();
        }

        if (Input.GetMouseButtonDown(0))
        {
            suggestedMatch = false;
            accTime = 0.0f;
            foreach (var block in suggestedMatchBlocks)
            {
                if (block.activeSelf)
                {
                    block.GetComponent<Animator>().SetTrigger("Reset");
                }
            }

            var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Block"))
            {
                var hitIdx = tileEntities.FindIndex(x => x == hit.collider.gameObject);
                var hitBlock = hit.collider.gameObject.GetComponent<TileEntity>();
                if (blockers[hitIdx] != null)
                {
                    return;
                }

                if (IsBoosterBlock(hitBlock))
                {
                    DestroyBooster(hitBlock);
                }
                else
                {
                    DestroyBlock(hitBlock);
                }
            }
        }
    }

    /// <summary>
    /// Spawns a new tile entity.
    /// </summary>
    /// <param name="go">The game object to spawn.</param>
    /// <returns>The spawned game object.</returns>
    private GameObject CreateBlock(GameObject go)
    {
        if (go == null)
        {
            Debug.Log("null");

            return go;
        }

        go.GetComponent<TileEntity>().Spawn();
        return go;
    }

    /// <summary>
    /// Destroys the specified block.
    /// </summary>
    /// <param name="blockToDestroy">The block to destroy.</param>
    private void DestroyBlock(TileEntity blockToDestroy)
    {
        var blockIdx = tileEntities.FindIndex(x => x == blockToDestroy.gameObject);
        var blocksToDestroy = new List<GameObject>();
        GetMatches(blockToDestroy.gameObject, blocksToDestroy);
        var score = 0;
        if (blocksToDestroy.Count > 0)
        {
            foreach (var block in blocksToDestroy)
            {
                var idx = tileEntities.FindIndex(x => x == block);
                if (blockers[idx] != null)
                {
                    score += DestroyIce(block, idx);
                }

                score += DestroyConnectedStones(idx);
                score += DestroyTileEntity(block.GetComponent<TileEntity>(), idx);
            }

            ShowScoreText(score, blockToDestroy.transform.position);
            UpdateScore(score);
            CreateBooster(blocksToDestroy.Count, blockIdx);
            PerformMove(true);
            StartCoroutine(ApplyGravityAsync());
        }
        else
        {
            blockToDestroy.GetComponent<Animator>().SetTrigger("NoMatches");
            SoundManager.Instance.PlaySound("CubePressError");
            ApplyPenalty();
        }
    }

    /// <summary>
    /// Applies the penalty for missing a match to the LevelData (if any).
    /// </summary>
    private void ApplyPenalty()
    {
        if (applyingPenalty || LevelData.penalty <= 0)
        {
            return;
        }

        applyingPenalty = true;
        StartCoroutine(AnimateLimitDown(LevelData.penalty));
    }

    /// <summary>
    /// Animates the limit text down to its value with the specified penalty applied.
    /// </summary>
    /// <param name="penalty">The penalty to apply to the limit.</param>
    /// <returns>The coroutine.</returns>
    private IEnumerator AnimateLimitDown(int penalty)
    {
        if (LevelData.limitType == LimitType.Time)
        {
            StopCoroutine(countdownCoroutine);
        }

        var endValue = currentLimit - penalty;
        if (LevelData.limitType == LimitType.Time)
        {
            endValue += 1;
        }

        while (currentLimit > 0 && currentLimit != endValue)
        {
            currentLimit -= 1;
            yield return new WaitForSeconds(0.1f);
        }


        applyingPenalty = false;
    }

    /// <summary>
    /// Destroys the specified booster.
    /// </summary>
    /// <param name="blockToDestroy">The booster to destroy.</param>
    private void DestroyBooster(TileEntity blockToDestroy)
    {
        var score = 0;

        var blocksToDestroy = new List<GameObject>();
        var usedBoosters = new List<GameObject>();
        DestroyBoosterRecursive(blockToDestroy, blocksToDestroy, usedBoosters);

        foreach (var block in blocksToDestroy)
        {
            var idx = tileEntities.FindIndex(x => x == block);
            if (blockers[idx] != null)
            {
                score += DestroyIce(block, idx);
            }

            score += DestroyTileEntity(block.GetComponent<TileEntity>(), idx, false);
        }

        ShowScoreText(score, blockToDestroy.transform.position);
        UpdateScore(score);
        PerformMove(true);
        StartCoroutine(ApplyGravityAsync());
    }

    /// <summary>
    /// Internal recursive method used to destroy the specified booster.
    /// </summary>
    /// <param name="blockToDestroy">The booster to destroy.</param>
    /// <param name="blocksToDestroy">The accumulated blocks that have been destroyed so far.</param>
    /// <param name="usedBoosters">The boosters already destroyed.</param>
    private void DestroyBoosterRecursive(TileEntity blockToDestroy, List<GameObject> blocksToDestroy,
        List<GameObject> usedBoosters)
    {
        var blockIdx = tileEntities.FindIndex(x => x == blockToDestroy.gameObject);
        var newBlocksToDestroy = blockToDestroy.GetComponent<Booster>().Resolve(this, blockIdx);
        usedBoosters.Add(blockToDestroy.gameObject);

        blockToDestroy.GetComponent<Booster>().ShowFx(gamePools, this, blockIdx);

        if (!currentlyAwardingBoosters)
        {
            foreach (var block in newBlocksToDestroy)
            {
                if (block.GetComponent<Booster>() != null && !usedBoosters.Contains(block))
                {
                    usedBoosters.Add(block);
                    DestroyBoosterRecursive(block.GetComponent<TileEntity>(), blocksToDestroy, usedBoosters);
                }
            }
        }

        foreach (var block in newBlocksToDestroy)
        {
            if (!blocksToDestroy.Contains(block))
            {
                blocksToDestroy.Add(block);
            }
        }
    }

    /// <summary>
    /// Destroys the specified ice.
    /// </summary>
    /// <param name="blocker">The ice to destroy.</param>
    /// <param name="idx">The index of the ice to destroy.</param>
    /// <returns>The score obtained by the destruction of the ice.</returns>
    private int DestroyIce(GameObject blocker, int idx)
    {
        var score = m_BlockScore;

        blockers[idx].GetComponent<PooledObject>().pool.ReturnObject(blockers[idx]);
        gameState.collectedBlockers[BlockerType.Ice] += 1;
        blockers[idx] = null;
        var particles = gamePools.iceParticlesPool.GetObject();
        if (particles != null)
        {
            particles.transform.position = blocker.transform.position;
            particles.GetComponent<TileParticles>().fragmentParticles.Play();
        }

        SoundManager.Instance.PlaySound("IceBreak");

        return score;
    }

    /// <summary>
    /// Destroys the specified tile entity.
    /// </summary>
    /// <param name="tileEntity">The tile entity to destroy.</param>
    /// <param name="tileIndex">The index of the tile entity to destroy.</param>
    /// <param name="playSound">True if the destruction sound should be played; false otherwise.</param>
    /// <returns>The score obtained by the destruction of the tile entity.</returns>
    private int DestroyTileEntity(TileEntity tileEntity, int tileIndex, bool playSound = true)
    {
        var block = tileEntity.GetComponent<Block>();
        if (block != null)
        {
            gameState.collectedBlocks[block.type] += 1;
        }

        var tileScore = m_BlockScore;

        var particles = gamePools.GetParticles(tileEntity);
        if (particles != null)
        {
            particles.transform.position = tileEntity.transform.position;
            particles.GetComponent<TileParticles>().fragmentParticles.Play();
        }

        if (block != null)
        {
            if (block.type == BlockType.Stone)
            {
                SoundManager.Instance.PlaySound("Stone");
            }
            else if (block.type == BlockType.Ball)
            {
                SoundManager.Instance.PlaySound("Ball");
            }
            else if (playSound)
            {
                SoundManager.Instance.PlaySound("CubePress");
            }
        }

        tileEntity.Explode();
        tileEntities[tileIndex] = null;
        tileEntity.GetComponent<PooledObject>().pool.ReturnObject(tileEntity.gameObject);
        return tileScore;
    }

    /// <summary>
    /// Updates the score of the current game.
    /// </summary>
    /// <param name="score">The score.</param>
    private void UpdateScore(int score)
    {
        gameState.score += score;
        gameUi.scoreText.text = gameState.score.ToString();
    }

    /// <summary>
    /// Resets the LevelData data. This is particularly useful when replaying a LevelData.
    /// </summary>
    public void ResetLevelData()
    {
        gameStarted = false;
        gameFinished = false;
        currentlyAwardingBoosters = false;

        generatedCollectables = 0;
        neededCollectables = 0;

        gameState.Reset();

        gameUi.limitText.text = "";
        gameUi.scoreText.text = gameState.score.ToString();

        foreach (var pool in gamePools.GetComponentsInChildren<ObjectPool>())
        {
            pool.Reset();
        }

        tileEntities.Clear();
        blockers.Clear();
        tilePositions.Clear();

        for (var j = 0; j < LevelData.height; j++)
        {
            for (var i = 0; i < LevelData.width; i++)
            {
                var tileIndex = i + (j * LevelData.width);
                var tileToGet = gamePools.GetTileEntity(LevelData, LevelData.tiles[tileIndex]);
                if (tileToGet == null)
                {
                    Debug.Log("null Here");
                }

                var tile = CreateBlock(tileToGet.gameObject);
                var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                blockWidth = spriteRenderer.bounds.size.x;
                blockHeight = spriteRenderer.bounds.size.y;
                tile.transform.position = new Vector2(i * (blockWidth + horizontalSpacing),
                    -j * (blockHeight + verticalSpacing));
                tileEntities.Add(tile);
                spriteRenderer.sortingOrder = LevelData.height - j;

                var block = tile.GetComponent<Block>();
                if (block != null && block.type == BlockType.Collectable)
                {
                    generatedCollectables += 1;
                }
            }
        }

        var totalWidth = (LevelData.width - 1) * (blockWidth + horizontalSpacing);
        var totalHeight = (LevelData.height - 1) * (blockHeight + verticalSpacing);
        foreach (var block in tileEntities)
        {
            var newPos = block.transform.position;
            newPos.x -= totalWidth / 2;
            newPos.y += totalHeight / 2;
            newPos.y += levelLocation.position.y;
            block.transform.position = newPos;
            tilePositions.Add(newPos);
        }

        for (var j = 0; j < LevelData.height; j++)
        {
            for (var i = 0; i < LevelData.width; i++)
            {
                var tileIndex = i + (j * LevelData.width);
                if (LevelData.tiles[tileIndex].blockerType == BlockerType.Ice)
                {
                    var cover = gamePools.icePool.GetObject();
                    cover.transform.position = tilePositions[tileIndex];
                    cover.GetComponent<SpriteRenderer>().sortingOrder = 10;
                    blockers.Add(cover);
                }
                else
                {
                    blockers.Add(null);
                }
            }
        }

        //    OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup => popup.SetGoals(LevelData.goal));
    }

    /// <summary>
    /// Creates the background tiles of the LevelData.
    /// </summary>
    private void CreateBackgroundTiles()
    {
        var backgroundTiles = new GameObject("BackgroundTiles");
        for (var j = 0; j < LevelData.height; j++)
        {
            for (var i = 0; i < LevelData.width; i++)
            {
                var tileIndex = i + (j * LevelData.width);
                var tile = LevelData.tiles[tileIndex];
                var blockTile = tile as BlockTile;
                if (blockTile != null && blockTile.type == BlockType.Empty)
                {
                    continue;
                }

                var go = new GameObject("Background");
                go.transform.parent = backgroundTiles.transform;
                var sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = backgroundSprite;
                sprite.color = backgroundColor;
                sprite.sortingLayerName = "Game";
                sprite.sortingOrder = -2;
                sprite.transform.position = tileEntities[tileIndex].transform.position;
            }
        }
    }

    /// <summary>
    /// Highlights a random match as a suggestion to the player when he is idle for some time.
    /// </summary>
    private void HighlightRandomMatch()
    {
        if (suggestedMatch)
        {
            return;
        }

        suggestedMatchBlocks.Clear();
        var tries = 0;
        do
        {
            var randomIdx = UnityEngine.Random.Range(0, tileEntities.Count);
            if (tileEntities[randomIdx] != null)
            {
                // Prevent infinite loops when no matches are possible.
                tries += 1;
                if (tries >= 100)
                {
                    break;
                }

                if (IsColorBlock(tileEntities[randomIdx].GetComponent<TileEntity>()))
                {
                    GetMatches(tileEntities[randomIdx], suggestedMatchBlocks);
                }

                // Prevent matches with all the blocks having blockers.
                var isValidMatch = false;
                foreach (var tile in suggestedMatchBlocks)
                {
                    var idx = tileEntities.FindIndex(x => x == tile);
                    if (blockers[idx] == null)
                    {
                        isValidMatch = true;
                        break;
                    }
                }

                if (!isValidMatch)
                {
                    suggestedMatchBlocks.Clear();
                }
            }
        } while (suggestedMatchBlocks.Count == 0);

        if (suggestedMatchBlocks.Count > 0)
        {
            foreach (var match in suggestedMatchBlocks)
            {
                match.GetComponent<Animator>().SetTrigger("SuggestedMatch");
            }

            suggestedMatch = true;
        }
        else
        {
            StartCoroutine(RegenerateLevel());
        }
    }

    /// <summary>
    /// Regenerates the LevelData when no matches are possible.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator RegenerateLevel()
    {
        yield return new WaitForSeconds(2.0f);
        for (var i = 0; i < LevelData.width; i++)
        {
            for (var j = 0; j < LevelData.height; j++)
            {
                var idx = i + (j * LevelData.width);
                var block = tileEntities[idx];
                if (block != null)
                {
                    if (IsColorBlock(block.GetComponent<TileEntity>()))
                    {
                        block.GetComponent<PooledObject>().pool.ReturnObject(block);
                        var newBlock = CreateNewBlock();
                        tileEntities[idx] = newBlock;
                        newBlock.transform.position = tilePositions[idx];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Destroys the stones connected to the tile entity at the specified index.
    /// </summary>
    /// <param name="idx">The index.</param>
    private int DestroyConnectedStones(int idx)
    {
        var score = 0;

        var i = idx % LevelData.width;
        var j = idx / LevelData.width;

        var topTile = new TileDef(i, j - 1);
        var bottomTile = new TileDef(i, j + 1);
        var leftTile = new TileDef(i - 1, j);
        var rightTile = new TileDef(i + 1, j);
        var surroundingTiles = new List<TileDef> { topTile, bottomTile, leftTile, rightTile };
        foreach (var surroundingTile in surroundingTiles)
        {
            if (IsValidTileEntity(surroundingTile))
            {
                var tileIndex = (LevelData.width * surroundingTile.y) + surroundingTile.x;
                var tile = tileEntities[tileIndex];
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && (block.type == BlockType.Stone || block.type == BlockType.Ball))
                    {
                        score += DestroyTileEntity(block, tileIndex);
                    }
                }
            }
        }

        return score;
    }

    /// <summary>
    /// Calculates the matches of the specified block.
    /// </summary>
    /// <param name="go">The block to check.</param>
    /// <param name="matchedTiles">A list containing the matched tiles.</param>
    private void GetMatches(GameObject go, List<GameObject> matchedTiles)
    {
        var idx = tileEntities.FindIndex(x => x == go);
        var i = idx % LevelData.width;
        var j = idx / LevelData.width;

        var topTile = new TileDef(i, j - 1);
        var bottomTile = new TileDef(i, j + 1);
        var leftTile = new TileDef(i - 1, j);
        var rightTile = new TileDef(i + 1, j);
        var surroundingTiles = new List<TileDef> { topTile, bottomTile, leftTile, rightTile };

        var hasMatch = false;
        foreach (var surroundingTile in surroundingTiles)
        {
            if (IsValidTileEntity(surroundingTile))
            {
                var tileIndex = (LevelData.width * surroundingTile.y) + surroundingTile.x;
                var tile = tileEntities[tileIndex];
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && block.type == go.GetComponent<Block>().type)
                    {
                        hasMatch = true;
                    }
                }
            }
        }

        if (!hasMatch)
        {
            return;
        }

        if (!matchedTiles.Contains(go))
        {
            matchedTiles.Add(go);
        }

        foreach (var surroundingTile in surroundingTiles)
        {
            if (IsValidTileEntity(surroundingTile))
            {
                var tileIndex = (LevelData.width * surroundingTile.y) + surroundingTile.x;
                var tile = tileEntities[tileIndex];
                if (tile != null)
                {
                    var block = tile.GetComponent<Block>();
                    if (block != null && block.type == go.GetComponent<Block>().type &&
                        !matchedTiles.Contains(tile))
                    {
                        GetMatches(tile, matchedTiles);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new booster at the specified index.
    /// </summary>
    /// <param name="numMatchedBlocks">The number of matched blocks.</param>
    /// <param name="blockIdx">The index of the block.</param>
    private void CreateBooster(int numMatchedBlocks, int blockIdx)
    {
        var eligibleBoosters = new List<KeyValuePair<BoosterType, int>>();
        // foreach (var pair in gameConfig.boosterNeededMatches)
        // {
        //     if (numMatchedBlocks >= pair.Value)
        //     {
        //         eligibleBoosters.Add(pair);
        //     }
        // }

        if (eligibleBoosters.Count > 0)
        {
            var max = eligibleBoosters.Max(x => x.Value);
            eligibleBoosters.RemoveAll(x => x.Value != max);
            var idx = UnityEngine.Random.Range(0, eligibleBoosters.Count);
            var booster = eligibleBoosters[idx];
            CreateBooster(GetBoosterPool(booster.Key).GetObject(), blockIdx);
        }
    }

    /// <summary>
    /// Returns the booster pool corresponding to the specified booster type.
    /// </summary>
    /// <param name="type">The booster type.</param>
    /// <returns>The booster pool corresponding to the specified booster type.</returns>
    private ObjectPool GetBoosterPool(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.HorizontalBomb:
                return gamePools.horizontalBombPool;

            case BoosterType.VerticalBomb:
                return gamePools.verticalBombPool;

            case BoosterType.Dynamite:
                return gamePools.dynamitePool;

            case BoosterType.ColorBomb:
                return gamePools.colorBombPool;
        }

        return null;
    }

    /// <summary>
    /// Creates the booster of the specified type at the specified index.
    /// </summary>
    /// <param name="type">The type of booster to create.</param>
    /// <param name="blockIdx">The index at which to create the booster.</param>
    private void CreateBooster(BoosterType type, int blockIdx)
    {
        ObjectPool boosterPool = null;
        switch (type)
        {
            case BoosterType.HorizontalBomb:
                boosterPool = gamePools.horizontalBombPool;
                break;

            case BoosterType.VerticalBomb:
                boosterPool = gamePools.verticalBombPool;
                break;

            case BoosterType.Dynamite:
                boosterPool = gamePools.dynamitePool;
                break;

            case BoosterType.ColorBomb:
                boosterPool = gamePools.colorBombPool;
                break;
        }

        var booster = CreateBlock(boosterPool.GetObject());
        CreateBooster(booster, blockIdx);
    }

    /// <summary>
    /// Creates a booster at the specified index.
    /// </summary>
    /// <param name="booster">The booster to create.</param>
    /// <param name="blockIdx">The index at which to create the booster.</param>
    private void CreateBooster(GameObject booster, int blockIdx)
    {
        booster.transform.position = tilePositions[blockIdx];
        tileEntities[blockIdx] = booster;
        var j = blockIdx / LevelData.height;
        booster.GetComponent<SpriteRenderer>().sortingOrder = LevelData.height - j;

        var particles = gamePools.boosterSpawnParticlesPool.GetObject();
        particles.AddComponent<AutoKillPooled>();
        particles.transform.position = booster.transform.position;
        foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
        {
            child.Play();
        }
    }

    /// <summary>
    /// Utility coroutine to apply the gravity to the LevelData.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ApplyGravityAsync()
    {
        yield return new WaitForSeconds(0.1f);
        ApplyGravity();
        yield return new WaitForSeconds(0.5f);
        CheckForCollectables();
        CheckEndGame();
    }

    /// <summary>
    /// Applies the gravity to the LevelData.
    /// </summary>
    private void ApplyGravity()
    {
        for (var i = 0; i < LevelData.width; i++)
        {
            for (var j = LevelData.height - 1; j >= 0; j--)
            {
                var tileIndex = i + (j * LevelData.width);
                if (tileEntities[tileIndex] == null ||
                    IsEmptyBlock(tileEntities[tileIndex].GetComponent<TileEntity>()) ||
                    IsStoneBlock(tileEntities[tileIndex].GetComponent<TileEntity>()))
                {
                    continue;
                }

                // Find bottom.
                var bottom = -1;
                for (var k = j; k < LevelData.height; k++)
                {
                    var idx = i + (k * LevelData.width);
                    if (tileEntities[idx] == null)
                    {
                        bottom = k;
                    }
                    else
                    {
                        var block = tileEntities[idx].GetComponent<Block>();
                        if (block != null && block.type == BlockType.Stone)
                        {
                            break;
                        }
                    }
                }

                if (bottom != -1)
                {
                    var tile = tileEntities[tileIndex];
                    if (tile != null)
                    {
                        var numTilesToFall = bottom - j;
                        tileEntities[tileIndex + (numTilesToFall * LevelData.width)] = tileEntities[tileIndex];
                        var tween = LeanTween.move(tile,
                            tilePositions[tileIndex + LevelData.width * numTilesToFall],
                            blockFallSpeed);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() =>
                        {
                            if (tile.activeSelf)
                            {
                                tile.GetComponent<Animator>().SetTrigger("Falling");
                            }
                        });
                        tileEntities[tileIndex] = null;
                    }
                }
            }
        }

        for (var i = 0; i < LevelData.width; i++)
        {
            var numEmpties = 0;
            for (var j = 0; j < LevelData.height; j++)
            {
                var idx = i + (j * LevelData.width);
                if (tileEntities[idx] == null)
                {
                    numEmpties += 1;
                }
                else
                {
                    var block = tileEntities[idx].GetComponent<Block>();
                    if (block != null && block.type == BlockType.Stone)
                    {
                        break;
                    }
                }
            }

            if (numEmpties > 0)
            {
                for (var j = 0; j < LevelData.height; j++)
                {
                    var tileIndex = i + (j * LevelData.width);
                    var isEmptyTile = false;
                    var isStoneTile = false;
                    if (tileEntities[tileIndex] != null)
                    {
                        var blockTile = tileEntities[tileIndex].GetComponent<Block>();
                        if (blockTile != null)
                        {
                            isEmptyTile = blockTile.type == BlockType.Empty;
                            isStoneTile = blockTile.type == BlockType.Stone;
                        }

                        if (isStoneTile)
                        {
                            break;
                        }
                    }

                    if (tileEntities[tileIndex] == null && !isEmptyTile)
                    {
                        var tile = CreateNewBlock();
                        var pos = tilePositions[i];
                        pos.y = tilePositions[i].y + (numEmpties * (blockHeight + verticalSpacing));
                        --numEmpties;
                        tile.transform.position = pos;
                        var tween = LeanTween.move(tile,
                            tilePositions[tileIndex],
                            blockFallSpeed);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() =>
                        {
                            if (tile.activeSelf)
                            {
                                tile.GetComponent<Animator>().SetTrigger("Falling");
                            }
                        });
                        tileEntities[tileIndex] = tile;
                    }

                    if (tileEntities[tileIndex] != null)
                    {
                        tileEntities[tileIndex].GetComponent<SpriteRenderer>().sortingOrder = LevelData.height - j;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new block.
    /// </summary>
    /// <returns>The newly created block.</returns>
    private GameObject CreateNewBlock()
    {
        var percent = UnityEngine.Random.Range(0, 100);
        if (generatedCollectables < neededCollectables &&
            percent < LevelData.collectableChance)
        {
            generatedCollectables += 1;
            return CreateBlock(gamePools.GetTileEntity(LevelData, new BlockTile { type = BlockType.Collectable })
                .gameObject);
        }
        else
        {
            return CreateBlock(gamePools.GetTileEntity(LevelData, new BlockTile { type = BlockType.RandomBlock })
                .gameObject);
        }
    }

    /// <summary>
    /// Checks if any collectable has been collected by the player.
    /// </summary>
    private void CheckForCollectables()
    {
        var collectablesToDestroy = new List<Block>();
        for (var i = 0; i < LevelData.width; i++)
        {
            Block bottom = null;
            var tileIndex = 0;
            for (var j = LevelData.height - 1; j >= 0; j--)
            {
                tileIndex = i + (j * LevelData.width);
                if (tileEntities[tileIndex] == null)
                {
                    continue;
                }

                var block = tileEntities[tileIndex].GetComponent<Block>();
                if (block != null)
                {
                    if (block.type == BlockType.Empty)
                    {
                        continue;
                    }

                    bottom = block;
                }

                break;
            }

            if (bottom != null && bottom.type == BlockType.Collectable)
            {
                collectablesToDestroy.Add(bottom);
                tileEntities[tileIndex] = null;
            }
        }

        if (collectablesToDestroy.Count > 0)
        {
            foreach (var tile in collectablesToDestroy)
            {
                gameState.collectedBlocks[tile.type] += 1;

                var score = 100;
                ShowScoreText(score, tile.transform.position);

                var particles = gamePools.GetParticles(tile);
                if (particles != null)
                {
                    particles.transform.position = tile.transform.position;
                    particles.GetComponent<TileParticles>().fragmentParticles.Play();
                }

                SoundManager.Instance.PlaySound("Collectable");

                tile.Explode();
                tile.GetComponent<PooledObject>().pool.ReturnObject(tile.gameObject);
            }

            PerformMove(false);
            StartCoroutine(ApplyGravityAsync());
        }
    }

    /// <summary>
    /// Updates the state of the game based on the last move performed by the player.
    /// </summary>
    /// <param name="updateLimits">True if the current limits of the LevelData should be updated; false otherwise.</param>
    private void PerformMove(bool updateLimits)
    {
        if (currentlyAwardingBoosters)
        {
            return;
        }

        if (LevelData.limitType == LimitType.Moves && updateLimits)
        {
            --currentLimit;
            if (currentLimit < 0)
            {
                currentLimit = 0;
            }
        }

        gameUi.goalUi.UpdateGoals(gameState);
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void StartGame()
    {
        currentLimit = LevelData.limit;
        if (LevelData.limitType == LimitType.Moves)
        {
            gameUi.limitTitleText.text = "Moves";
        }
        else if (LevelData.limitType == LimitType.Time)
        {
            gameUi.limitTitleText.text = "Time";
        }

        gameUi.SetGoals(LevelData.goal, goalPrefab);

        gameStarted = true;
    }

    /// <summary>
    /// Ends the current game.
    /// </summary>
    private void EndGame()
    {
        gameFinished = true;
        RestartGame();
    }

    /// <summary>
    /// Restarts the current game.
    /// </summary>
    public void RestartGame()
    {
        ResetLevelData();
    }

    /// <summary>
    /// Checks if the game has finished.
    /// </summary>
    private void CheckEndGame()
    {
        if (currentlyAwardingBoosters)
        {
            return;
        }

        if (gameFinished)
        {
            return;
        }

        var goalsComplete = LevelData.goal.IsComplete(gameState);

        if (goalsComplete)
        {
            EndGame();
        }
        else
        {
            if (gameFinished)
            {
                StartCoroutine(OpenNoMovesOrTimePopupAsync());
            }
        }
    }

    /// <summary>
    /// Opens the popup for buying additional moves or time.
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator OpenNoMovesOrTimePopupAsync()
    {
        yield return new WaitForSeconds(endGamePopupDelay);
        RestartGame();
    }

    private bool IsValidTileEntity(TileDef tileEntity)
    {
        return tileEntity.x >= 0 && tileEntity.x < LevelData.width &&
               tileEntity.y >= 0 && tileEntity.y < LevelData.height;
    }

    private bool IsColorBlock(TileEntity tileEntity)
    {
        var block = tileEntity as Block;
        return block != null &&
               (block.type == BlockType.Block1 ||
                block.type == BlockType.Block2 ||
                block.type == BlockType.Block3 ||
                block.type == BlockType.Block4 ||
                block.type == BlockType.Block5 ||
                block.type == BlockType.Block6);
    }

    private bool IsBoosterBlock(TileEntity tileEntity)
    {
        return tileEntity is Booster;
    }

    private bool IsEmptyBlock(TileEntity tileEntity)
    {
        var block = tileEntity as Block;
        return block != null && block.type == BlockType.Empty;
    }

    /// <summary>
    /// Returns true if the specified tile entity is a stone block and false otherwise.
    /// </summary>
    /// <param name="tileEntity">The tile entity.</param>
    /// <returns>True if the specified tile entity is a stone block; false otherwise.</returns>
    private bool IsStoneBlock(TileEntity tileEntity)
    {
        var block = tileEntity as Block;
        return block != null && block.type == BlockType.Stone;
    }

    private void ShowScoreText(int score, Vector2 pos)
    {
        var scoreText = scoreTextPool.GetObject();
        var canvasRect = canvas.GetComponent<RectTransform>();
        scoreText.transform.SetParent(canvas.transform, false);
        scoreText.GetComponent<Text>().text = string.Format("+{0}", score);
        var viewportPos = mainCamera.WorldToViewportPoint(pos);
        var screenPos = new Vector2(
            (viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
        scoreText.GetComponent<RectTransform>().anchoredPosition = screenPos;
        scoreText.GetComponent<ScoreText>().StartAnimation();
    }

    private void FadeOutInGameBoosterOverlay()
    {
        LeanTween.cancel(ingameBoosterBgTweenId, false);
        var tween = LeanTween.value(ingameBoosterPanel.gameObject, 1.0f, 0.0f, 0.2f).setOnUpdate(value =>
        {
            ingameBoosterPanel.GetComponent<CanvasGroup>().alpha = value;
            ingameBoosterText.GetComponent<CanvasGroup>().alpha = value;
        });
        tween.setOnComplete(() => ingameBoosterPanel.GetComponent<CanvasGroup>().blocksRaycasts = false);
    }
}