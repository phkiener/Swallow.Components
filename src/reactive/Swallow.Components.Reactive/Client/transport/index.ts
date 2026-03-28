import * as common from "../common";

export interface Transport {
    initialize(fragment: common.Fragment): Promise<void>;
    send(interaction: common.Interaction): Promise<void>;
    close(): Promise<void>;
}
