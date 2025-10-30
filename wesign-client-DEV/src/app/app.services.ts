import { UserApiService } from "@services/user-api.service"; import { ModalService } from "@services/modal.service";
import { AppConfigService } from '@services/app-config.service';
import { SharedService } from '@services/shared.service';
import { GroupAssignService } from '@services/group-assign.service';

const SERVICES: any[] = [
    UserApiService,
    ModalService,
    SharedService,
    GroupAssignService
];

export { SERVICES };
