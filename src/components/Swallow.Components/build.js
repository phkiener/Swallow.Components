import { bundle } from "lightningcss";
import { build } from "tsdown";
import * as fs from "node:fs";

const targetDirectory = process.env.OUT_DIR;

// scripts
const rootScript = "client/js/root.ts";
if (fs.existsSync(rootScript)) {
    await build({
        entry: rootScript,
        format: "esm",
        outDir: targetDirectory,
        dts: false,
        minify: true,
        platform: "browser",
    });
}

// styles
const rootStyle = "client/css/root.css";
if (fs.existsSync(rootStyle)) {
    let { code } = await bundle({ filename: rootStyle, monify: true });

    const targetFile = targetDirectory + "styles.css";
    await fs.writeFileSync(targetFile, code);
}
