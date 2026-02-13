import React from 'react';
import { fireEvent, screen } from '@testing-library/react';
import { ReportDesigner } from '../../components/analytics/ReportDesigner';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

describe('ReportDesigner', () => {
  it('shows an error when report name is empty', () => {
    renderWithProviders(<ReportDesigner onSave={jest.fn()} />);

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: '' } });

    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report name is required/i)).toBeInTheDocument();
  });

  it('shows an error when report name is not unique', () => {
    renderWithProviders(
      <ReportDesigner
        onSave={jest.fn()}
        existingReportNames={['Sales Pipeline']}
      />
    );

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: 'Sales Pipeline' } });

    fireEvent.click(screen.getByText(/opportunity name/i));
    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report name must be unique/i)).toBeInTheDocument();
  });

  it('shows an error when report query is missing', () => {
    renderWithProviders(<ReportDesigner onSave={jest.fn()} />);

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: 'Unique Report' } });

    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report query is required/i)).toBeInTheDocument();
  });
});
