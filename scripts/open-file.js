const fs = require('fs');
const path = require('path');
const { spawnSync } = require('child_process');

const rel = process.argv[2];
if (!rel) {
  console.error('Usage: node scripts/open-file.js <relative-path>');
  process.exit(1);
}

const file = path.resolve(__dirname, '..', rel);
if (!fs.existsSync(file)) {
  console.error(`File not found: ${file}`);
  process.exit(1);
}

console.log(`Opening ${file}`);

const openCli = path.join(
  __dirname,
  '..',
  'node_modules',
  '.bin',
  process.platform === 'win32' ? 'open-cli.cmd' : 'open-cli'
);

const result = spawnSync(openCli, [file], {
  stdio: 'inherit',
  shell: process.platform === 'win32',
});

process.exit(result.status ?? 1);
