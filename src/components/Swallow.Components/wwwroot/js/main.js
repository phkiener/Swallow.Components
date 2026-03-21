import registerTabs from "./components/tabs.js"
import { processTree } from "./lib/idref.js"

registerTabs(document.body);
processTree(document.body);
