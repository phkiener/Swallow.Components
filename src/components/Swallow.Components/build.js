import { bundle } from "lightningcss";
import * as fs from "node:fs";

const sourceFile = "styles/root.css";
if (!fs.existsSync(sourceFile)) {
    process.exit(0);
}

let { code } = await bundle({ filename: sourceFile, monify: true });

const targetFile = process.env.OUT_DIR + "styles.css";
await fs.writeFileSync(targetFile, code);
