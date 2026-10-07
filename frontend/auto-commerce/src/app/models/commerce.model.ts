export interface CommerceRow {
  pc_processdate: string;
  pc_nomcomred: string;
  pc_numdoc: string;
  nom_vehiculo: string;
  marca: string;
  modelo: string;
  anio: string;
  placa: string;
  color: string;
  tipo_vehiculo: string;
  kilometraje: string;
  precio: string;
  estado_vehiculo: string;
}

export interface QuarantineRow {
  pcProcessdate: string;
  pcNomcomred: string | null;
  pcNumdoc: string | null;
  nomVehiculo: string | null;
  marca: string | null;
  modelo: string | null;
  anio: number | null;
  placa: string | null;
  color: string | null;
  tipoVehiculo: string | null;
  kilometraje: number | null;
  precio: number | null;
  estadoVehiculo: string | null;
  motivo: string;
}