import { BenchmarkResult } from "./axios/types";

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

export interface ComputeSupport {
  name: string,
  technologyId: number,
  isSupported: boolean,
  version: string,
  details: string
}

interface CPUWorkload {
  "UsagePercent": number,
  "ProcessCount": number,
  "ThreadCount": number,
  "CurrentClockSpeed": number
}

interface RAMWorkload {
  "TotalGb": number,
  "UsedGb": number,
  "FreeGb": number,
  "UsagePercent": number
}

export interface GPUWorkload {
  "Name": string,
  "MemoryUsedMb": number,
  "MemoryTotalMb": number
}

export interface Workload {
  CPU: CPUWorkload;
  RAM: RAMWorkload;
  GPUs: GPUWorkload[];
}

export interface Experiment<T> {
  name: string;
  time: number;
  inputData: T;
  results: BenchmarkResult[];
}