"use client";

import { useState, useEffect } from "react";
import api from "@/lib/api";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { AlertCircle, CheckCircle, Power } from "lucide-react";
import { toast } from "sonner";

export default function AdminPage() {
  const [isEnabled, setIsEnabled] = useState(true);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadStatus();
  }, []);

  const loadStatus = async () => {
    try {
      const response = await api.get("/systemcontrol");
      if (response.data.success) {
        setIsEnabled(response.data.data.isEnabled);
        setMessage(response.data.data.message || "");
      }
    } catch (error) {
      toast.error("Erro ao carregar status do sistema");
    }
  };
  const handleToggle = async () => {
    try {
      setLoading(true);
      const response = await api.put("/systemcontrol/toggle", {
        isEnabled,
        message:
          message ||
          (isEnabled
            ? "Sistema operando normalmente"
            : "Sistema desativado - Entre em contato com o suporte"),
      });
      if (response.data.success) {
        toast.success(response.data.message);
      }
    } catch (error) {
      toast.error("Erro ao atualizar status do sistema");
    } finally {
      setLoading(false);
    }
  };
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Administração do Sistema</h1>
        <p className="text-muted-foreground">Controle total do sistema</p>
      </div>
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Power className="h-5 w-5" />
            Kill Switch - Controle do Sistema
          </CardTitle>
          <CardDescription>
            Desative o sistema completamente em caso de inadimplência ou
            manutenção
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Status Atual */}
          <div
            className={`p-4 rounded-lg border-2 ${
              isEnabled
                ? "bg-green-50 border-green-200 dark:bg-green-950 dark:border-green-800"
                : "bg-red-50 border-red-200 dark:bg-red-950 dark:border-red-800"
            }`}
          >
            <div className="flex items-center gap-3">
              {isEnabled ? (
                <>
                  <CheckCircle className="h-6 w-6 text-green-600 dark:text-green-400" />
                  <div>
                    <p className="font-semibold text-green-900 dark:text-green-100">
                      Sistema Ativo
                    </p>
                    <p className="text-sm text-green-700 dark:text-green-300">
                      O sistema está operando normalmente
                    </p>
                  </div>
                </>
              ) : (
                <>
                  <AlertCircle className="h-6 w-6 text-red-600 dark:text-red-400" />
                  <div>
                    <p className="font-semibold text-red-900 dark:text-red-100">
                      Sistema Desativado
                    </p>
                    <p className="text-sm text-red-700 dark:text-red-300">
                      Todas as requisições estão sendo bloqueadas
                    </p>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Toggle */}
          <div className="flex items-center justify-between p-4 border rounded-lg">
            <div className="flex-1">
              <Label
                htmlFor="system-toggle"
                className="text-base font-semibold"
              >
                Status do Sistema
              </Label>
              <p className="text-sm text-muted-foreground mt-1">
                Ative ou desative o acesso ao sistema
              </p>
            </div>
            <Switch
              id="system-toggle"
              checked={isEnabled}
              onCheckedChange={setIsEnabled}
              className="data-[state=checked]:bg-green-500"
            />
          </div>

          {/* Mensagem Customizada */}
          <div className="space-y-2">
            <Label htmlFor="message">Mensagem para Usuários</Label>
            <Textarea
              id="message"
              placeholder="Digite a mensagem que será exibida aos usuários quando o sistema estiver desativado..."
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              rows={4}
              className="resize-none"
            />
            <p className="text-xs text-muted-foreground">
              Esta mensagem será exibida quando o sistema estiver desativado
            </p>
          </div>

          {/* Botão de Salvar */}
          <div className="flex justify-end gap-4">
            <Button variant="outline" onClick={loadStatus} disabled={loading}>
              Cancelar
            </Button>
            <Button onClick={handleToggle} disabled={loading}>
              {loading ? "Salvando..." : "Salvar Alterações"}
            </Button>
          </div>

          {/* Aviso */}
          <div className="p-4 bg-yellow-50 dark:bg-yellow-950 border border-yellow-200 dark:border-yellow-800 rounded-lg">
            <div className="flex gap-3">
              <AlertCircle className="h-5 w-5 text-yellow-600 dark:text-yellow-400 flex-shrink-0 mt-0.5" />
              <div className="text-sm text-yellow-900 dark:text-yellow-100">
                <p className="font-semibold mb-1">Atenção!</p>
                <p>
                  Ao desativar o sistema, todos os usuários serão impedidos de
                  acessar qualquer funcionalidade. Use esta função apenas em
                  casos de inadimplência, manutenção crítica ou emergências.
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
