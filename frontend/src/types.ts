export interface OSInfo {
  name: string;
  architecture: string;
  version: string;
}

export interface RamInfo {
  manufacturer: string;
  partNumber: string;
  memorySize: number;
  clockSpeed: number;
  count: number;
  totalMemorySize: number;
}

export interface CPUInfo {
  name: string;
  physicalCores: number;
  logicalCores: number;
  maxClockSpeed: string;
}

export interface GPUInfo {
  name: string;
  driverVersion: string;
  memory: string;
}

export interface SystemDataInfo {
  os: OSInfo;
  rams: RamInfo[];
  cpu: CPUInfo;
  gpus: GPUInfo[];
}