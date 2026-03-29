import type { IkonUiModuleLoader, IkonUiRegistry } from '@ikonai/sdk-react-ui';
import { createPdfViewerResolver } from './pdf-viewer';

export const loadPdfViewerModule: IkonUiModuleLoader = () => [createPdfViewerResolver()];

export function registerPdfViewerModule(registry: IkonUiRegistry): void {
  registry.registerModule('pdf-viewer', loadPdfViewerModule);
}
