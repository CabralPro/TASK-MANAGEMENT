const { spawnSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const repoRoot = path.join(__dirname, '..');
const resultsDir = path.join(repoRoot, 'TestResults');
const reportDir = path.join(repoRoot, 'CoverageReport');

function run(command, args, options = {}) {
  const result = spawnSync(command, args, {
    cwd: repoRoot,
    stdio: 'inherit',
    shell: false,
    ...options,
  });

  if (result.status !== 0) {
    process.exit(result.status ?? 1);
  }
}

function removeDir(dir) {
  if (!fs.existsSync(dir)) {
    return;
  }

  try {
    fs.rmSync(dir, { recursive: true, force: true });
  } catch (error) {
    console.error(
      `Unable to clear ${dir}. Close any open coverage report pages/tabs and try again.\n${error.message}`
    );
    process.exit(1);
  }
}

function findCoverageFiles(dir) {
  const matches = [];

  if (!fs.existsSync(dir)) {
    return matches;
  }

  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const entryPath = path.join(dir, entry.name);

    if (entry.isDirectory()) {
      matches.push(...findCoverageFiles(entryPath));
      continue;
    }

    if (entry.name === 'coverage.cobertura.xml') {
      matches.push(entryPath);
    }
  }

  return matches;
}

function findRunningWebApiPids() {
  if (process.platform === 'win32') {
    const result = spawnSync(
      'powershell.exe',
      [
        '-NoProfile',
        '-Command',
        "Get-CimInstance Win32_Process -Filter \"Name = 'dotnet.exe'\" | Where-Object { $_.CommandLine -match 'TaskManagement\\.WebAPI' } | Select-Object -ExpandProperty ProcessId"
      ],
      { encoding: 'utf8' }
    );

    return (result.stdout || '')
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter(Boolean);
  }

  const result = spawnSync('pgrep', ['-f', 'TaskManagement.WebAPI'], { encoding: 'utf8' });
  return (result.stdout || '')
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
}

function assertWebApiNotRunning() {
  const pids = findRunningWebApiPids();
  if (pids.length === 0) {
    return;
  }

  console.error(
    [
      'TaskManagement.WebAPI is still running and locks build outputs.',
      `Stop npm start / the API process (PID: ${pids.join(', ')}) before running coverage.`,
      'Then retry: npm run coverage:open'
    ].join('\n')
  );
  process.exit(1);
}

assertWebApiNotRunning();
removeDir(resultsDir);
removeDir(reportDir);

console.log('Restoring ReportGenerator dotnet tool...');
run('dotnet', ['tool', 'restore']);

console.log('Running backend tests with coverage...');
run('dotnet', [
  'test',
  'TaskManagement.sln',
  '--collect',
  'XPlat Code Coverage',
  '--results-directory',
  resultsDir,
  '--settings',
  'coverlet.runsettings',
  '--verbosity',
  'minimal',
]);

const coverageFiles = findCoverageFiles(resultsDir);
if (coverageFiles.length === 0) {
  console.error('No coverage.cobertura.xml files were produced.');
  process.exit(1);
}

console.log('Generating HTML coverage report...');
run('dotnet', [
  'tool',
  'run',
  'reportgenerator',
  `-reports:${coverageFiles.join(';')}`,
  `-targetdir:${reportDir}`,
  '-reporttypes:Html;TextSummary;Badges',
  '-assemblyfilters:+TaskManagement.*;-*.Tests',
  '-classfilters:-*Migrations*;-*Designer*;-*ModelSnapshot*;-*Program*;-*Startup*;-*OpenApi*;-*Setup*;-*DTOs*;-*Models.ApiResponse*;-*Abstractions*;-*Exceptions*;-*DependencyInjection*;-*Mappings*;-*Entity*;-*IAggregateRoot*;-*SignInRequest*;-*SignInResponse*;-*RegisterRequest*;-*CreateTaskRequest*;-*UpdateTaskRequest*;-*TaskDto*;-*AuthResponse*;-*AuthenticatedUser*;-*TaskStatus*',
]);

const summaryPath = path.join(reportDir, 'Summary.txt');
if (fs.existsSync(summaryPath)) {
  console.log('\n===== Coverage Summary =====\n');
  console.log(fs.readFileSync(summaryPath, 'utf8'));
}

console.log(`\nHTML report: ${path.join(reportDir, 'index.html')}`);
