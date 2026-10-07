import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { CommerceService } from '../../services/commerce.service';
import {
  CommerceRow,
  QuarantineRow
} from '../../models/commerce.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  selectedFile: File | null = null;

  previewRows: CommerceRow[] = [];
  quarantineRows: QuarantineRow[] = [];

  processDate = '';

  databaseConnected = false;

  uploading = false;
  processing = false;
  loadingQuarantine = false;

  uploadCompleted = false;

  totalLoaded = 0;
  totalQuarantine = 0;
  totalValid = 0;

  message = '';
  messageType: 'success' | 'error' | '' = '';

  constructor(
    private commerceService: CommerceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.checkDatabase();
    this.loadQuarantine();
  }


  // =========================================================
  // ESTADO DE CONEXION
  // =========================================================

  checkDatabase(): void {
    this.commerceService
      .getDatabaseStatus()
      .subscribe({
        next: () => {
          this.databaseConnected = true;
          this.cdr.detectChanges();
        },
        error: () => {
          this.databaseConnected = false;
          this.cdr.detectChanges();
        }
      });
  }


  // =========================================================
  // SELECCION / DRAG & DROP DEL CSV
  // =========================================================

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();

    const file = event.dataTransfer?.files?.[0];

    if (file) {
      this.handleFile(file);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    const file = input.files?.[0];

    if (file) {
      this.handleFile(file);
    }
  }

  private handleFile(file: File): void {
    this.resetMessage();

    // Formato obligatorio:
    // commerce_DDMMYYYY.csv
    // Ejemplo:
    // commerce_07102026.csv
    const validName =
      /^commerce_\d{8}\.csv$/i.test(file.name);

    if (!validName) {
      this.selectedFile = null;
      this.previewRows = [];

      this.showError(
        'El archivo debe utilizar el formato commerce_DDMMYYYY.csv.'
      );

      this.cdr.detectChanges();

      return;
    }

    if (file.size === 0) {
      this.selectedFile = null;
      this.previewRows = [];

      this.showError(
        'El archivo seleccionado se encuentra vacio.'
      );

      this.cdr.detectChanges();

      return;
    }

    this.selectedFile = file;
    this.uploadCompleted = false;

    this.readCsvPreview(file);
  }


  // =========================================================
  // PREVISUALIZACION DEL CSV
  // =========================================================

  private readCsvPreview(file: File): void {
    const reader = new FileReader();

    reader.onload = () => {
      const text =
        reader.result?.toString() ?? '';

      const lines = text
        .split(/\r?\n/)
        .filter(line => line.trim() !== '');

      if (lines.length <= 1) {
        this.previewRows = [];
        this.totalLoaded = 0;
        this.totalValid = 0;

        this.showError(
          'El archivo CSV no contiene registros.'
        );

        this.cdr.detectChanges();

        return;
      }

      const headers =
        this.splitCsvLine(lines[0])
          .map(header =>
            header
              .replace(/^\uFEFF/, '')
              .trim()
              .toLowerCase()
          );

      const requiredHeaders = [
        'pc_processdate',
        'pc_nomcomred',
        'pc_numdoc',
        'nom_vehiculo',
        'marca',
        'modelo',
        'anio',
        'placa',
        'color',
        'tipo_vehiculo',
        'kilometraje',
        'precio',
        'estado_vehiculo'
      ];

      const structureIsValid =
        requiredHeaders.every(
          header => headers.includes(header)
        );

      if (!structureIsValid) {
        this.previewRows = [];
        this.totalLoaded = 0;
        this.totalValid = 0;

        this.showError(
          'La estructura del archivo CSV no es valida.'
        );

        this.cdr.detectChanges();

        return;
      }

      this.previewRows =
        lines
          .slice(1)
          .map(line => {
            const values =
              this.splitCsvLine(line);

            const record: Record<string, string> = {};

            headers.forEach(
              (header, index) => {
                record[header] =
                  values[index]?.trim() ?? '';
              }
            );

            return record as unknown as CommerceRow;
          });

      this.totalLoaded =
        this.previewRows.length;

      // Antes del procesamiento consideramos
      // todos los registros como pendientes/validos.
      this.totalValid =
        this.previewRows.length;

      if (this.previewRows.length > 0) {
        this.processDate =
          this.previewRows[0].pc_processdate;
      }

      this.showSuccess(
        `${this.previewRows.length} registros listos para previsualizar.`
      );

      this.cdr.detectChanges();
    };

    reader.onerror = () => {
      this.previewRows = [];

      this.showError(
        'No fue posible leer el archivo CSV.'
      );

      this.cdr.detectChanges();
    };

    reader.readAsText(file);
  }


  // =========================================================
  // LECTOR SIMPLE DE LINEAS CSV
  // SOPORTA CAMPOS ENTRE COMILLAS
  // =========================================================

  private splitCsvLine(line: string): string[] {
    const values: string[] = [];

    let current = '';
    let insideQuotes = false;

    for (let i = 0; i < line.length; i++) {
      const char = line[i];

      if (char === '"') {

        if (
          insideQuotes &&
          line[i + 1] === '"'
        ) {
          current += '"';
          i++;
        } else {
          insideQuotes = !insideQuotes;
        }

      } else if (
        char === ',' &&
        !insideQuotes
      ) {

        values.push(current);
        current = '';

      } else {

        current += char;
      }
    }

    values.push(current);

    return values;
  }


  // =========================================================
  // SUBIR CSV AL BACKEND
  // =========================================================

  uploadCsv(): void {
    if (!this.selectedFile) {
      this.showError(
        'Seleccione primero un archivo CSV.'
      );

      this.cdr.detectChanges();

      return;
    }

    if (this.uploadCompleted) {
      this.showError(
        'Este archivo ya fue enviado al servidor.'
      );

      this.cdr.detectChanges();

      return;
    }

    this.uploading = true;
    this.resetMessage();

    this.commerceService
      .uploadCsv(this.selectedFile)
      .subscribe({
        next: response => {
          this.uploading = false;
          this.uploadCompleted = true;

          this.totalLoaded =
            response.registrosInsertados ?? 0;

          this.totalValid =
            this.totalLoaded;

          this.showSuccess(
            `${this.totalLoaded} registros fueron cargados correctamente.`
          );

          this.cdr.detectChanges();
        },

        error: error => {
          this.uploading = false;

          this.showError(
            error.error?.message ??
            'No fue posible cargar el archivo.'
          );

          this.cdr.detectChanges();
        }
      });
  }


  // =========================================================
  // PROCESAR REGISTROS POR FECHA
  // =========================================================

  processCommerce(): void {
    if (!this.processDate) {
      this.showError(
        'Seleccione una fecha de proceso.'
      );

      this.cdr.detectChanges();

      return;
    }

    if (!this.uploadCompleted) {
      this.showError(
        'Primero debe enviar el archivo CSV al servidor.'
      );

      this.cdr.detectChanges();

      return;
    }

    this.processing = true;
    this.resetMessage();

    this.commerceService
      .processCommerce(this.processDate)
      .subscribe({
        next: response => {
          this.processing = false;

          const newQuarantineCount =
            response.registrosCuarentena ?? 0;

          this.totalValid =
            Math.max(
              this.totalLoaded -
              newQuarantineCount,
              0
            );

          this.showSuccess(
            `Proceso finalizado. ${newQuarantineCount} registros fueron enviados a cuarentena.`
          );

          // Volvemos a consultar el GET real,
          // para mostrar todos los registros rechazados.
          this.loadQuarantine();

          this.cdr.detectChanges();
        },

        error: error => {
          this.processing = false;

          this.showError(
            error.error?.message ??
            'No fue posible procesar los registros.'
          );

          this.cdr.detectChanges();
        }
      });
  }


  // =========================================================
  // CONSULTAR CUARENTENA
  // =========================================================

  loadQuarantine(): void {
    this.loadingQuarantine = true;

    this.commerceService
      .getQuarantine()
      .subscribe({
        next: response => {
          this.loadingQuarantine = false;

          this.quarantineRows =
            response.data ?? [];

          this.totalQuarantine =
            response.total ?? 0;

          this.cdr.detectChanges();
        },

        error: () => {
          this.loadingQuarantine = false;

          this.cdr.detectChanges();
        }
      });
  }


  // =========================================================
  // LIMPIAR SELECCION ACTUAL
  // =========================================================

  clearSelection(): void {
    this.selectedFile = null;

    this.previewRows = [];

    this.processDate = '';

    this.uploadCompleted = false;

    this.totalLoaded = 0;
    this.totalValid = 0;

    this.resetMessage();

    this.cdr.detectChanges();
  }


  // =========================================================
  // MENSAJES
  // =========================================================

  private showSuccess(
    message: string
  ): void {
    this.message = message;
    this.messageType = 'success';
  }

  private showError(
    message: string
  ): void {
    this.message = message;
    this.messageType = 'error';
  }

  private resetMessage(): void {
    this.message = '';
    this.messageType = '';
  }
}