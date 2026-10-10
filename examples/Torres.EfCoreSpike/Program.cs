using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DotNetEnv;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Torres.EfCoreSpike.Persistence;

const string ShowSqlArgument = "--sql";
const string SqlLogIndent = "    ";
const string UsernameIndex = "ux_player_username_lower";
const string TestUsername = "efcore_prueba";
const string TestEmail = "efcore_prueba@example.com";
const string DuplicateEmail = "otro_correo@example.com";
const string TestPasswordHash = "hash-de-prueba";
const string TestAvatarReference = "avatar_03";
const string PassedLabel = "OK";
const string FailedLabel = "FALLA";
const string CheckLinePattern = "[{0}] {1}: {2}";
const string CountCheckName = "Contar jugadores";
const string CountDetailPattern = "EF Core cuenta {0} jugadores";
const string ReadCheckName = "Leer y mapear columnas";
const string EmptyTableDetail = "la tabla está vacía";
const string ReadDetailPattern = "#{0} {1} <{2}>, created_at {3:O}";
const string SearchCheckName = "Buscar sin distinguir mayúsculas";
const string SearchDetailPattern = "«{0}» encontró a #{1}";
const string NobodyFound = "nadie";
const string InsertCheckName = "Insertar con identidad y valor por defecto";
const string InsertDetailPattern = "la base asignó player_id {0} y created_at {1:O}";
const string DuplicateCheckName = "Detectar nombre repetido";
const string DuplicateDetailPattern = "índice violado: {0}";
const string NoViolation = "ninguno";
const string UpdateCheckName = "Actualizar después del error";
const string UpdateDetailPattern = "avatar_reference = {0}";
const string DeleteCheckName = "Borrar";
const string RowStillPresent = "la fila sigue ahí";
const string RowDeleted = "la fila ya no existe";
const string RollbackCheckName = "Revertir la transacción";
const string RowLeftOver = "quedó la fila de prueba";
const string DatabaseUnchanged = "la base quedó como estaba";
const string AllPassedMessage = "Todas las comprobaciones pasaron.";
const string FailuresMessagePattern = "{0} comprobaciones fallaron.";
const int SuccessExitCode = 0;
const int FailureExitCode = 1;

Env.TraversePath().Load();

bool isSqlShown = args.Contains(ShowSqlArgument);
Action<string>? sqlLogger = isSqlShown ? WriteSqlLine : null;
DbContextOptions<TorresDbContext> contextOptions = DatabaseSettings.CreateOptions(sqlLogger);
int failedCheckCount = 0;

await using (var readContext = new TorresDbContext(contextOptions))
{
    long playerCount = await readContext.Players.LongCountAsync();
    ReportCheck(CountCheckName, playerCount >= 0, FormatText(CountDetailPattern, playerCount));

    Player? firstPlayer = await readContext.Players.AsNoTracking().OrderBy(player => player.PlayerId).FirstOrDefaultAsync();
    bool isFirstPlayerMapped = (firstPlayer is not null) && (firstPlayer.CreatedAt.Kind == DateTimeKind.Utc);
    string readDetail = firstPlayer is null
        ? EmptyTableDetail
        : FormatText(ReadDetailPattern, firstPlayer.PlayerId, firstPlayer.Username, firstPlayer.Email, firstPlayer.CreatedAt);
    ReportCheck(ReadCheckName, isFirstPlayerMapped, readDetail);

    if (firstPlayer is not null)
    {
        string upperCaseUsername = firstPlayer.Username.ToUpperInvariant();
        Player? foundPlayer = await FindByUsernameAsync(readContext, upperCaseUsername);
        string foundPlayerId = foundPlayer?.PlayerId.ToString(CultureInfo.InvariantCulture) ?? NobodyFound;
        bool isSamePlayer = foundPlayer?.PlayerId == firstPlayer.PlayerId;
        ReportCheck(SearchCheckName, isSamePlayer, FormatText(SearchDetailPattern, upperCaseUsername, foundPlayerId));
    }
}

await using (var writeContext = new TorresDbContext(contextOptions))
{
    await using IDbContextTransaction testTransaction = await writeContext.Database.BeginTransactionAsync();

    var createdPlayer = new Player
    {
        Username = TestUsername,
        Email = TestEmail,
        PasswordHash = TestPasswordHash,
    };
    writeContext.Players.Add(createdPlayer);
    await writeContext.SaveChangesAsync();
    bool isInsertGenerated = (createdPlayer.PlayerId > 0) && (createdPlayer.CreatedAt != default);
    ReportCheck(InsertCheckName, isInsertGenerated, FormatText(InsertDetailPattern, createdPlayer.PlayerId, createdPlayer.CreatedAt));

    var duplicatePlayer = new Player
    {
        Username = TestUsername.ToUpperInvariant(),
        Email = DuplicateEmail,
        PasswordHash = TestPasswordHash,
    };
    writeContext.Players.Add(duplicatePlayer);
    string? violatedIndex = null;
    try
    {
        await writeContext.SaveChangesAsync();
    }
    catch (DbUpdateException exception)
        when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } uniqueViolation)
    {
        violatedIndex = uniqueViolation.ConstraintName;
        writeContext.Entry(duplicatePlayer).State = EntityState.Detached;
    }

    ReportCheck(DuplicateCheckName, violatedIndex == UsernameIndex, FormatText(DuplicateDetailPattern, violatedIndex ?? NoViolation));

    createdPlayer.AvatarReference = TestAvatarReference;
    await writeContext.SaveChangesAsync();
    string? storedAvatar = await writeContext.Players
        .Where(player => player.PlayerId == createdPlayer.PlayerId)
        .Select(player => player.AvatarReference)
        .SingleAsync();
    ReportCheck(UpdateCheckName, storedAvatar == TestAvatarReference, FormatText(UpdateDetailPattern, storedAvatar));

    writeContext.Players.Remove(createdPlayer);
    await writeContext.SaveChangesAsync();
    bool isRowStillPresent = await writeContext.Players.AnyAsync(player => player.PlayerId == createdPlayer.PlayerId);
    string deleteDetail = isRowStillPresent ? RowStillPresent : RowDeleted;
    ReportCheck(DeleteCheckName, !isRowStillPresent, deleteDetail);

    await testTransaction.RollbackAsync();
}

await using (var verifyContext = new TorresDbContext(contextOptions))
{
    bool isTestRowLeftOver = await verifyContext.Players.AnyAsync(player => player.Username == TestUsername);
    string rollbackDetail = isTestRowLeftOver ? RowLeftOver : DatabaseUnchanged;
    ReportCheck(RollbackCheckName, !isTestRowLeftOver, rollbackDetail);
}

string summaryMessage = failedCheckCount == 0 ? AllPassedMessage : FormatText(FailuresMessagePattern, failedCheckCount);
Console.WriteLine(summaryMessage);
int exitCode = failedCheckCount == 0 ? SuccessExitCode : FailureExitCode;

return exitCode;

void ReportCheck(string checkName, bool isPassed, string checkDetail)
{
    string resultLabel = isPassed ? PassedLabel : FailedLabel;
    string checkLine = FormatText(CheckLinePattern, resultLabel, checkName, checkDetail);
    Console.WriteLine(checkLine);
    if (!isPassed)
    {
        failedCheckCount++;
    }
}

static void WriteSqlLine(string sqlMessage)
{
    string indentedSql = SqlLogIndent + sqlMessage.Trim();
    Console.WriteLine(indentedSql);
}

static string FormatText(string textPattern, params object?[] textArguments)
{
    string formattedText = string.Format(CultureInfo.InvariantCulture, textPattern, textArguments);

    return formattedText;
}

static Task<Player?> FindByUsernameAsync(TorresDbContext searchContext, string username)
{
    string loweredUsername = username.ToLowerInvariant();
    Task<Player?> playerSearch = searchContext.Players.AsNoTracking().SingleOrDefaultAsync(player => player.Username.ToLower() == loweredUsername);

    return playerSearch;
}
