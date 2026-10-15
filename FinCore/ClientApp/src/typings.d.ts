/* SystemJS module definition */
declare const module: NodeModule;
interface NodeModule {
  id: string;
}

declare module "*.json" {
  const value: any;
  export default value;
}
