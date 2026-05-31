import { bundle } from "lightningcss";
import { build } from "tsdown";
import * as fs from "node:fs";

// scripts
const rootScript = "scripts/root.ts";
if (fs.existsSync(rootScript)) {
    await build({
        entry: rootScript,
        format: "esm",
        outDir: process.env.OUT_DIR,
        dts: false,
        minify: true,
        platform: "browser",
    });
}

// styles
const rootStyle = "styles/root.css";
if (fs.existsSync(rootStyle)) {
    let { code } = await bundle({ filename: rootStyle, monify: true });

    const targetFile = process.env.OUT_DIR + "styles.css";
    await fs.writeFileSync(targetFile, code);
}
