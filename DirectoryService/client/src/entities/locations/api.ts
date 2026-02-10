import { Location } from "@/entities/locations/type";
import { apiClient } from "@/shared/api/axios-instance";

export type GetLocationsRequest = {
    departmentsIds?: string[];
    search?: string;
    isActive?: boolean;
    sortBy?: string;
    page?: number;
    pageSize?: number;
};

export type LocationsResult = {
    locations: Location[];
    totalCount: number;
};

/*export type Envelope<T = unknown> = {
    result: T | null;
    error: ApiError | null;
    isError: boolean;
    timeGenerated: string;
};

export type ApiError = {
    message: ErrorMesage[];
    type: ErrorType;
};

export type ErrorMesage = {
    code: string;
    message: string;
    invalidField?: string | null;
};

export type ErrorType =
    | "validation"
    | "not_found"
    | "failure"
    | "conflict"
    | "authentification"
    | "autorization";*/

export const locationsApi = {
    getLocations: async (
        request: GetLocationsRequest,
    ): Promise<LocationsResult> => {
        const response = await apiClient.get<LocationsResult>("/locations", {
            params: request,
        });

        return response.data;
    },
};
